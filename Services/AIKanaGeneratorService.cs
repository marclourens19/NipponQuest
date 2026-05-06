using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NipponQuest.Data;
using NipponQuest.Models;

namespace NipponQuest.Services
{
    public class AIKanaGeneratorService : BackgroundService
    {
        private readonly ILogger<AIKanaGeneratorService> _log;
        private readonly IServiceProvider _services;
        private readonly HttpClient _http = new HttpClient();

        // ── CONFIGURATION – read from env vars (set in your deployment platform) ──
        private static readonly string Endpoint = Environment.GetEnvironmentVariable("AI_ENDPOINT")      // e.g. https://api.anthropic.com/v1/messages
                                                   ?? throw new InvalidOperationException("AI_ENDPOINT env var missing");
        private static readonly string ModelName = Environment.GetEnvironmentVariable("AI_MODEL")         // e.g. claude-3-5-sonnet-20240620
                                                   ?? "claude-3-5-sonnet-20240620";
        private static readonly string ApiKey = Environment.GetEnvironmentVariable("AI_API_KEY")        // token for the provider
                                                   ?? throw new InvalidOperationException("AI_API_KEY env var missing");

        private const int BatchSize = 200;   // how many words per alphabet/difficulty

        public AIKanaGeneratorService(ILogger<AIKanaGeneratorService> log,
                                      IServiceProvider services)
        {
            _log = log;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run once a day at 02:30 UTC (adjust schedule as you like)
            var nextRun = DateTime.UtcNow.Date.AddHours(2.5);
            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = nextRun - DateTime.UtcNow;
                if (delay > TimeSpan.Zero) await Task.Delay(delay, stoppingToken);

                try
                {
                    await GenerateAndPersistAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "AI Kana generation failed");
                }

                nextRun = nextRun.AddDays(1); // schedule next day
            }
        }

        private async Task GenerateAndPersistAsync(CancellationToken token)
        {
            var alphabets = new[] { "hiragana", "katakana", "dakuten", "mixed" };
            var difficulties = new[] { "normal", "hard", "insanity" };

            using var scope = _services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            foreach (var alpha in alphabets)
                foreach (var diff in difficulties)
                {
                    token.ThrowIfCancellationRequested();

                    var prompt = BuildPrompt(alpha, diff);
                    var requestPayload = BuildProviderPayload(prompt);

                    var httpReq = new HttpRequestMessage(HttpMethod.Post, Endpoint)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(requestPayload),
                                                     Encoding.UTF8, "application/json")
                    };
                    // Most providers use a bearer token header – adjust if your provider differs.
                    httpReq.Headers.Add("Authorization", $"Bearer {ApiKey}");

                    // Some providers (Anthropic, OpenAI) also need a version header
                    if (Endpoint.Contains("anthropic.com"))
                        httpReq.Headers.Add("anthropic-version", "2023-06-01");

                    var resp = await _http.SendAsync(httpReq, token);
                    resp.EnsureSuccessStatusCode();

                    var json = await resp.Content.ReadAsStringAsync(token);
                    var aiWords = ExtractAiWords(json); // see helper below

                    // ---- VALIDATION & INSERT ----
                    var insertBatch = new List<KanaWord>();
                    foreach (var w in aiWords)
                    {
                        if (!ValidateAiWord(w, alpha, diff)) continue;

                        bool exists = await ctx.KanaWords.AnyAsync(x =>
                            x.WordKana == w.Kana &&
                            x.Alphabet == alpha &&
                            x.DifficultyLevel == diff, token);
                        if (exists) continue;

                        insertBatch.Add(new KanaWord
                        {
                            WordKana = w.Kana,
                            WordRomaji = w.Romaji,
                            MeaningEnglish = w.Meaning,
                            Alphabet = alpha,
                            DifficultyLevel = diff,
                            CategoryTag = "AI‑Generated",
                            MissingKana = w.Missing,
                            DisplayHtml = w.Display,
                            IsAIGenerated = true,
                            GeneratedAt = DateTime.UtcNow
                        });
                    }

                    if (insertBatch.Count > 0)
                    {
                        await ctx.KanaWords.AddRangeAsync(insertBatch, token);
                        await ctx.SaveChangesAsync(token);
                        _log.LogInformation("Inserted {Count} AI‑generated {Alpha}/{Diff} words.", insertBatch.Count, alpha, diff);
                    }
                }
        }

        // ----------------- Helper methods -----------------

        private string BuildPrompt(string alphabet, string difficulty)
        {
            return $@"
  You are a Japanese‑language teacher. Generate {BatchSize} **unique** words for the {alphabet} script at **{difficulty}** difficulty.
  Return a **single JSON array** where each element has:
    - ""kana"": the full kana (or Kanji+furigana) string,
    - ""romaji"": exact romanisation (lowercase, no spaces),
    - ""meaning"": short English definition (1‑3 words),
    - ""missing"": one random syllable (or furigana piece) that will be blanked in the UI,
    - ""display"": same as ""kana"" but with the ""missing"" part replaced by <span class='missing-placeholder'>__</span>.

  All objects must be **valid JSON**, non‑duplicate, and the romaji must match the kana. Return only the JSON array, no extra text.";
        }

        private object BuildProviderPayload(string prompt)
        {
            // The payload differs per provider – we handle the three most common ones.
            if (Endpoint.Contains("vercel.com"))
            {
                // Vercel AI Gateway payload
                return new
                {
                    model = ModelName,
                    prompt = prompt,
                    temperature = 0.7,
                    max_tokens = 2048
                };
            }
            else if (Endpoint.Contains("anthropic.com"))
            {
                // Anthropic Claude payload (messages array)
                return new
                {
                    model = ModelName,
                    max_tokens = 2048,
                    temperature = 0.7,
                    messages = new[]
                    {
                          new { role = "system", content = "You are a helpful assistant." },
                          new { role = "user",   content = prompt }
                      }
                };
            }
            else if (Endpoint.Contains("huggingface.co"))
            {
                // HF Inference API payload
                return new
                {
                    inputs = prompt,
                    parameters = new { temperature = 0.7, max_new_tokens = 1024 }
                };
            }
            else
            {
                throw new NotSupportedException($"Endpoint '{Endpoint}' not recognised for payload building.");
            }
        }

        // Extract the JSON array the model returned – each provider nests it slightly differently.
        private List<AiKanaDto> ExtractAiWords(string rawJson)
        {
            // 1️⃣ Vercel – rawJson is the array itself.
            if (Endpoint.Contains("vercel.com"))
                return JsonSerializer.Deserialize<List<AiKanaDto>>(rawJson)!;

            // 2️⃣ Anthropic – response shape: { content: [{ text: "..." }], ... }
            if (Endpoint.Contains("anthropic.com"))
            {
                using var doc = JsonDocument.Parse(rawJson);
                var text = doc.RootElement
                              .GetProperty("content")[0]
                              .GetProperty("text")
                              .GetString()!;
                return JsonSerializer.Deserialize<List<AiKanaDto>>(text)!;
            }

            // 3️⃣ HuggingFace – response shape: [{ generated_text: "..." }]
            if (Endpoint.Contains("huggingface.co"))
            {
                using var doc = JsonDocument.Parse(rawJson);
                var generated = doc.RootElement[0].GetProperty("generated_text").GetString()!;
                return JsonSerializer.Deserialize<List<AiKanaDto>>(generated)!;
            }

            throw new NotSupportedException($"Cannot parse response from {Endpoint}");
        }

        private bool ValidateAiWord(AiKanaDto w, string alphabet, string diff)
        {
            // Very lightweight sanity checks – you can extend them later.
            if (string.IsNullOrWhiteSpace(w.Kana) ||
                string.IsNullOrWhiteSpace(w.Romaji) ||
                string.IsNullOrWhiteSpace(w.Missing) ||
                string.IsNullOrWhiteSpace(w.Display))
                return false;

            // The missing piece must actually appear in the full kana string.
            if (!w.Kana.Contains(w.Missing)) return false;

            // Optional: you can put a small transliteration library here to confirm
            // that `w.Romaji` truly matches `w.Kana`. For now we trust the model.
            return true;
        }

        // DTO that matches what the model returns
        private class AiKanaDto
        {
            public string Kana { get; set; } = "";
            public string Romaji { get; set; } = "";
            public string Meaning { get; set; } = "";
            public string Missing { get; set; } = "";
            public string Display { get; set; } = "";
        }
    }
}