using System.IO.Compression;
using Microsoft.Data.Sqlite;
using NipponQuest.Models;

namespace NipponQuest.Services
{
    public static class AnkiProcessor
    {
        public static List<Flashcard> GetCardsFromPackage(string packagePath, string workDir)
        {
            var cards = new List<Flashcard>();

            try
            {
                DeepSafeLog("Starting Anki extraction...");

                // Safety cleanup (only if safe)
                if (Directory.Exists(workDir))
                {
                    try
                    {
                        Directory.Delete(workDir, true);
                    }
                    catch
                    {
                        // ignore lock issues
                    }
                }

                Directory.CreateDirectory(workDir);

                // =========================
                // ZIP EXTRACTION (SAFE)
                // =========================
                try
                {
                    ZipFile.ExtractToDirectory(packagePath, workDir, overwriteFiles: true);
                }
                catch (Exception ex)
                {
                    throw new Exception("ZIP extraction failed", ex);
                }

                // =========================
                // FIND DB FILE
                // =========================
                string dbFile = Path.Combine(workDir, "collection.anki2");

                if (!File.Exists(dbFile))
                    dbFile = Path.Combine(workDir, "collection.anki21");

                if (!File.Exists(dbFile))
                    throw new Exception("No Anki database found (anki2/anki21 missing)");

                // =========================
                // SAFE COPY
                // =========================
                string safeDb = Path.Combine(workDir, "safe.db");

                File.Copy(dbFile, safeDb, true);

                if (new FileInfo(safeDb).Length == 0)
                    throw new Exception("Copied DB is empty");

                // =========================
                // SQLITE READ
                // =========================
                string connStr = $"Data Source={safeDb};Mode=ReadOnly;Pooling=False;";

                using (var sqlite = new SqliteConnection(connStr))
                {
                    sqlite.Open();

                    using (var cmd = sqlite.CreateCommand())
                    {
                        cmd.CommandText = "SELECT flds FROM notes";

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader.IsDBNull(0))
                                    continue;

                                var raw = reader.GetString(0);
                                if (string.IsNullOrWhiteSpace(raw))
                                    continue;

                                var fields = raw.Split((char)0x1f);

                                if (fields.Length >= 2)
                                {
                                    cards.Add(new Flashcard
                                    {
                                        FrontText = fields[0],
                                        BackText = fields[1]
                                    });
                                }
                            }
                        }
                    }
                }

                SqliteConnection.ClearAllPools();

                DeepSafeLog($"Extraction complete: {cards.Count} cards");

                return cards;
            }
            catch (Exception ex)
            {
                DeepSafeLog("AnkiProcessor FAILED: " + ex);
                throw;
            }
        }

        private static void DeepSafeLog(string msg)
        {
            Console.WriteLine($"[ANKI] {DateTime.Now:HH:mm:ss} {msg}");
        }
    }
}