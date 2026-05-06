@using NipponQuest.Controllers
@model NipponQuest.Controllers.StatsController.StatsViewModel
@{
    ViewData["Title"] = "Stats Vault";
    var user = Model.User;
    var dailyJson = System.Text.Json.JsonSerializer.Serialize(
        Model.DailyRewards.Select(r => new {
            d = r.Date.ToString("MM-dd"),
            iso = r.Date.ToString("yyyy-MM-dd"),
            exp = r.Exp,
            gold = r.Gold
        })
    );
    var calendarJson = System.Text.Json.JsonSerializer.Serialize(
        Model.Calendar.Select(c => new { d = c.Date.ToString("yyyy-MM-dd"), n = c.Count })
    );
    var blitzJson = System.Text.Json.JsonSerializer.Serialize(
        Model.BlitzRows.Select(r => new {
            difficulty = r.Difficulty,
            cells = r.Cells.Select(c => new { a = c.Alphabet, acc = Math.Round(c.Accuracy * 100) })
        })
    );
}

<style>
    /* ── Stats Page (KanaBlitz aesthetic) ─────────────────── */
    .reveal-up { opacity: 0; transform: translateY(30px); transition: all 0.6s cubic-bezier(0.16, 1, 0.3, 1); }
    .reveal-up.active { opacity: 1; transform: translateY(0); }

    .stats-hero {
        background: linear-gradient(135deg, #0f172a 0%, #1e293b 60%, #0ea5e9 140%);
        border-radius: 28px;
        color: #ffffff;
        padding: 2rem;
        position: relative;
        overflow: hidden;
        box-shadow: 0 25px 60px -12px rgba(15, 23, 42, 0.35);
    }
    .stats-hero::before {
        content: "";
        position: absolute;
        inset: -40% -10% auto auto;
        width: 320px; height: 320px;
        background: radial-gradient(circle, rgba(245, 158, 11, 0.18) 0%, transparent 70%);
        pointer-events: none;
    }
    .stats-hero .gamertag {
        font-size: clamp(1.6rem, 3vw, 2.2rem);
        font-weight: 900;
        letter-spacing: -1px;
        line-height: 1;
    }
    .stats-hero .level-chip {
        background: #fbbf24; color: #422006;
        font-weight: 900; letter-spacing: 1px;
        padding: 4px 10px; border-radius: 8px;
        font-size: 0.7rem; text-transform: uppercase;
    }
    .stats-hero .hero-meta {
        font-size: 0.75rem; font-weight: 800; letter-spacing: 1px;
        color: rgba(255,255,255,0.7); text-transform: uppercase;
    }
    .stats-hero .hero-stat .val { font-size: 1.6rem; font-weight: 900; line-height: 1; }
    .stats-hero .hero-stat .lbl { font-size: 0.65rem; font-weight: 800; letter-spacing: 1.5px; color: rgba(255,255,255,0.6); text-transform: uppercase; margin-top: 4px; }

    .progress-thin {
        height: 8px; background: rgba(255,255,255,0.15); border-radius: 50px; overflow: hidden;
    }
    .progress-thin > div {
        height: 100%; background: linear-gradient(90deg, #fbbf24, #f59e0b);
        border-radius: 50px;
    }

    .quick-stat {
        background: #ffffff; border: 1px solid #e2e8f0; border-radius: 18px;
        padding: 1.1rem 1.2rem; transition: all 250ms ease;
        height: 100%;
    }
    .quick-stat:hover { transform: translateY(-3px); box-shadow: 0 12px 28px rgba(15, 23, 42, 0.08); border-color: #cbd5e1; }
    .quick-stat .qs-icon {
        width: 40px; height: 40px; border-radius: 12px;
        display: inline-flex; align-items: center; justify-content: center;
        font-size: 1.1rem; margin-bottom: 10px;
    }
    .quick-stat .qs-val { font-size: 1.6rem; font-weight: 900; color: #0f172a; line-height: 1; }
    .quick-stat .qs-lbl { font-size: 0.6rem; font-weight: 900; letter-spacing: 1.5px; color: #94a3b8; text-transform: uppercase; margin-top: 6px; }
    .quick-stat .qs-sub { font-size: 0.7rem; font-weight: 700; color: #64748b; margin-top: 4px; }

    .panel-card {
        background: #ffffff; border: 1px solid #e2e8f0; border-radius: 24px;
        padding: 1.5rem; box-shadow: 0 8px 24px rgba(15, 23, 42, 0.04);
    }
    .panel-card .panel-title {
        font-size: 0.7rem; font-weight: 900; letter-spacing: 2px;
        color: #0f172a; text-transform: uppercase; margin-bottom: 1rem;
    }
    .panel-card .panel-sub { font-size: 0.75rem; color: #64748b; font-weight: 700; }

    .range-toggle .btn {
        background: #f1f5f9; border: none; color: #64748b;
        font-weight: 900; font-size: 0.7rem; letter-spacing: 1.5px; text-transform: uppercase;
        padding: 6px 14px; border-radius: 50px; transition: all 200ms ease;
    }
    .range-toggle .btn.active { background: #0f172a; color: #ffffff; }

    /* Mini reward badges (matches KanaBlitz) */
    .mini-xp {
        width: 22px; height: 22px; border-radius: 6px;
        background: #38bdf8; color: #ffffff;
        display: inline-flex; align-items: center; justify-content: center;
        font-size: 0.55rem; font-weight: 900;
        box-shadow: 0 2px 0 #0284c7;
    }
    .mini-gold {
        width: 22px; height: 22px; border-radius: 50%;
        background: #fbbf24; border: 2px solid #d97706; color: #92400e;
        display: inline-flex; align-items: center; justify-content: center;
        font-size: 0.6rem; font-weight: 900;
        box-shadow: 0 2px 0 #b45309;
    }

    /* Mastery donut center */
    .donut-wrap { position: relative; width: 100%; max-width: 220px; margin: 0 auto; }
    .donut-center {
        position: absolute; inset: 0;
        display: flex; flex-direction: column;
        align-items: center; justify-content: center;
        pointer-events: none;
    }
    .donut-center .v { font-size: 1.8rem; font-weight: 900; color: #0f172a; line-height: 1; }
    .donut-center .l { font-size: 0.6rem; font-weight: 900; letter-spacing: 1.5px; color: #94a3b8; text-transform: uppercase; margin-top: 4px; }

    /* Blitz accuracy matrix */
    .blitz-matrix {
        display: grid;
        grid-template-columns: 110px repeat(4, 1fr);
        gap: 8px;
    }
    .blitz-matrix .bm-head, .blitz-matrix .bm-row-head {
        font-size: 0.62rem; font-weight: 900; letter-spacing: 1.2px;
        color: #94a3b8; text-transform: uppercase;
        display: flex; align-items: center;
    }
    .blitz-matrix .bm-row-head { color: #0f172a; }
    .blitz-matrix .bm-cell {
        background: #f8fafc; border: 1px solid #e2e8f0;
        border-radius: 12px; padding: 14px 10px;
        text-align: center; font-weight: 900; color: #0f172a;
        font-size: 0.95rem;
        position: relative; overflow: hidden;
    }
    .blitz-matrix .bm-cell.empty { color: #cbd5e1; background: #ffffff; }
    .blitz-matrix .bm-cell.tier-low    { background: #fef2f2; border-color: #fecaca; color: #b91c1c; }
    .blitz-matrix .bm-cell.tier-mid    { background: #fffbeb; border-color: #fde68a; color: #b45309; }
    .blitz-matrix .bm-cell.tier-high   { background: #f0fdf4; border-color: #bbf7d0; color: #15803d; }
    .blitz-matrix .bm-cell.tier-elite  { background: linear-gradient(135deg, #ecfdf5 0%, #d1fae5 100%); border-color: #6ee7b7; color: #047857; }
    .blitz-matrix .bm-cell .pct-bar {
        position: absolute; left: 0; right: 0; bottom: 0; height: 3px;
        background: currentColor; opacity: 0.4;
    }

    /* Mastery distribution row */
    .mastery-row { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; margin-top: 14px; }
    .mastery-row .mr-item {
        text-align: center; padding: 12px; border-radius: 14px;
        border: 1px solid #e2e8f0; background: #f8fafc;
    }
    .mastery-row .mr-item .v { font-size: 1.2rem; font-weight: 900; color: #0f172a; }
    .mastery-row .mr-item .l { font-size: 0.6rem; font-weight: 900; letter-spacing: 1px; color: #94a3b8; text-transform: uppercase; margin-top: 2px; }

    /* Insights */
    .insight-list { list-style: none; padding: 0; margin: 0; }
    .insight-list li {
        display: flex; gap: 12px; align-items: flex-start;
        padding: 12px 14px; border-radius: 14px;
        background: #f8fafc; border: 1px solid #e2e8f0;
        margin-bottom: 8px;
        font-size: 0.85rem; font-weight: 700; color: #475569;
    }
    .insight-list li i {
        color: #f59e0b; font-size: 1rem; flex-shrink: 0; margin-top: 2px;
    }

    /* Activity calendar (last 30 days) */
    .heat-calendar {
        display: grid;
        grid-template-columns: repeat(15, minmax(0, 1fr));
        gap: 4px;
    }
    .heat-cell {
        aspect-ratio: 1 / 1;
        border-radius: 6px;
        background: #f1f5f9;
        border: 1px solid #e2e8f0;
    }
    .heat-cell.l1 { background: #dbeafe; border-color: #bfdbfe; }
    .heat-cell.l2 { background: #93c5fd; border-color: #60a5fa; }
    .heat-cell.l3 { background: #3b82f6; border-color: #2563eb; }
    .heat-cell.l4 { background: #1d4ed8; border-color: #1e40af; }

    /* Top decks list */
    .deck-row {
        display: flex; align-items: center; gap: 12px;
        padding: 12px 14px; border-radius: 14px; margin-bottom: 8px;
        border: 1px solid #e2e8f0; background: #ffffff;
        transition: all 200ms ease;
    }
    .deck-row:hover { transform: translateY(-2px); box-shadow: 0 8px 18px rgba(15, 23, 42, 0.06); }
    .deck-row .deck-color {
        width: 36px; height: 36px; border-radius: 10px;
        border: 2px solid #e2e8f0; flex-shrink: 0;
    }
    .deck-row .deck-title { font-weight: 900; color: #0f172a; font-size: 0.9rem; line-height: 1.2; }
    .deck-row .deck-sub { font-size: 0.7rem; color: #64748b; font-weight: 700; }
    .deck-row .mastery-bar {
        height: 6px; background: #e2e8f0; border-radius: 50px; overflow: hidden;
        margin-top: 6px;
    }
    .deck-row .mastery-bar > div {
        height: 100%; background: linear-gradient(90deg, #22c55e, #16a34a);
        border-radius: 50px;
    }

    .info-bubble-btn {
        width: 36px; height: 36px; border-radius: 50%;
        background: #ffffff; border: 2px solid #e2e8f0; color: #64748b;
        display: inline-flex; align-items: center; justify-content: center;
        transition: all 200ms ease;
    }
    .info-bubble-btn:hover { border-color: #0ea5e9; color: #0ea5e9; }
</style>

<div class="container py-5">

    <!-- HERO -->
    <div class="stats-hero mb-4 reveal-up">
        <div class="row g-4 align-items-center">
            <div class="col-md-6">
                <div class="hero-meta mb-2">
                    <i class="bi bi-graph-up-arrow me-1"></i> STATS VAULT
                </div>
                <div class="d-flex align-items-center gap-3 mb-3 flex-wrap">
                    <span class="gamertag">@(string.IsNullOrEmpty(user.GamerTag) ? user.UserName : user.GamerTag)</span>
                    <span class="level-chip">LV @user.Level</span>
                </div>

                <div class="d-flex flex-wrap gap-4 mb-3">
                    <div class="hero-stat">
                        <div class="val">@user.TotalEXP.ToString("N0")</div>
                        <div class="lbl">Total XP</div>
                    </div>
                    <div class="hero-stat">
                        <div class="val">@user.Gold.ToString("N0")</div>
                        <div class="lbl">Gold</div>
                    </div>
                    <div class="hero-stat">
                        <div class="val">@user.LoginStreak</div>
                        <div class="lbl">Day Streak</div>
                    </div>
                    <div class="hero-stat">
                        <div class="val">@user.LessonsCompleted</div>
                        <div class="lbl">Lessons</div>
                    </div>
                </div>

                <div class="hero-meta mb-1">
                    Level Progress
                    <span class="ms-2" style="color:#fbbf24;">@user.CurrentXP / @user.RequiredXP XP</span>
                </div>
                <div class="progress-thin">
                    <div style="width: @(Math.Min(100, user.RequiredXP > 0 ? (user.CurrentXP * 100 / user.RequiredXP) : 0))%;"></div>
                </div>
            </div>

            <div class="col-md-6">
                <div class="row g-3">
                    <div class="col-6">
                        <div class="p-3 rounded-4" style="background: rgba(255,255,255,0.08); border: 1px solid rgba(255,255,255,0.12);">
                            <div class="hero-meta">League</div>
                            <div class="fw-black fs-5 mt-1">@user.CurrentLeague.ToString().ToUpper()</div>
                        </div>
                    </div>
                    <div class="col-6">
                        <div class="p-3 rounded-4" style="background: rgba(255,255,255,0.08); border: 1px solid rgba(255,255,255,0.12);">
                            <div class="hero-meta">Cards Mastered</div>
                            <div class="fw-black fs-5 mt-1">@Model.CardsMastered <span class="opacity-75 fs-6">/ @Model.TotalCards</span></div>
                        </div>
                    </div>
                    <div class="col-6">
                        <div class="p-3 rounded-4" style="background: rgba(255,255,255,0.08); border: 1px solid rgba(255,255,255,0.12);">
                            <div class="hero-meta">Blitz Accuracy</div>
                            <div class="fw-black fs-5 mt-1">@Math.Round(Model.OverallBlitzAccuracy * 100)%</div>
                        </div>
                    </div>
                    <div class="col-6">
                        <div class="p-3 rounded-4" style="background: rgba(255,255,255,0.08); border: 1px solid rgba(255,255,255,0.12);">
                            <div class="hero-meta">Due Today</div>
                            <div class="fw-black fs-5 mt-1">@Model.CardsDueToday</div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- QUICK STATS GRID -->
    <div class="row g-3 mb-4 reveal-up">
        <div class="col-md-2 col-6">
            <div class="quick-stat">
                <div class="qs-icon" style="background:#eff6ff; color:#2563eb;"><i class="bi bi-card-list"></i></div>
                <div class="qs-val">@Model.CardsLearned</div>
                <div class="qs-lbl">Cards Learned</div>
                <div class="qs-sub">of @Model.TotalCards total</div>
            </div>
        </div>
        <div class="col-md-2 col-6">
            <div class="quick-stat">
                <div class="qs-icon" style="background:#f0fdf4; color:#16a34a;"><i class="bi bi-trophy-fill"></i></div>
                <div class="qs-val">@Model.CardsMastered</div>
                <div class="qs-lbl">Mastered</div>
                <div class="qs-sub">5+ correct streaks</div>
            </div>
        </div>
        <div class="col-md-2 col-6">
            <div class="quick-stat">
                <div class="qs-icon" style="background:#fef3c7; color:#b45309;"><i class="bi bi-collection-fill"></i></div>
                <div class="qs-val">@Model.TotalDecks</div>
                <div class="qs-lbl">Total Decks</div>
                <div class="qs-sub">@Model.CommunityClones from community</div>
            </div>
        </div>
        <div class="col-md-2 col-6">
            <div class="quick-stat">
                <div class="qs-icon" style="background:#fff7ed; color:#ea580c;"><i class="bi bi-fire"></i></div>
                <div class="qs-val">@Model.ReviewedToday</div>
                <div class="qs-lbl">Today</div>
                <div class="qs-sub">cards reviewed</div>
            </div>
        </div>
        <div class="col-md-2 col-6">
            <div class="quick-stat">
                <div class="qs-icon" style="background:#fef2f2; color:#dc2626;"><i class="bi bi-bullseye"></i></div>
                <div class="qs-val">@Model.CardsDueToday</div>
                <div class="qs-lbl">Due Now</div>
                <div class="qs-sub">awaiting review</div>
            </div>
        </div>
        <div class="col-md-2 col-6">
            <div class="quick-stat">
                <div class="qs-icon" style="background:#f5f3ff; color:#7c3aed;"><i class="bi bi-megaphone-fill"></i></div>
                <div class="qs-val">@Model.PublishedDecks</div>
                <div class="qs-lbl">Published</div>
                <div class="qs-sub">decks in market</div>
            </div>
        </div>
    </div>

    <!-- ACTIVITY + REWARDS -->
    <div class="row g-4 mb-4 reveal-up">
        <div class="col-lg-8">
            <div class="panel-card h-100">
                <div class="d-flex justify-content-between align-items-start flex-wrap gap-2 mb-2">
                    <div>
                        <div class="panel-title">XP &amp; Gold Earned</div>
                        <div class="panel-sub">Tracking how productivity translates into rewards.</div>
                    </div>
                    <div class="range-toggle d-flex gap-1" role="group">
                        <button type="button" class="btn active" data-range="week">Week</button>
                        <button type="button" class="btn" data-range="month">Month</button>
                    </div>
                </div>

                <div class="d-flex flex-wrap gap-3 mt-3 mb-2">
                    <div class="d-flex align-items-center gap-2">
                        <span class="mini-xp">XP</span>
                        <span class="fw-black" id="range-exp">@Model.WeeklyExp.ToString("N0")</span>
                        <span class="text-muted small fw-bold" id="range-exp-lbl">this week</span>
                    </div>
                    <div class="d-flex align-items-center gap-2">
                        <span class="mini-gold">G</span>
                        <span class="fw-black" id="range-gold">@Model.WeeklyGold.ToString("N0")</span>
                        <span class="text-muted small fw-bold" id="range-gold-lbl">this week</span>
                    </div>
                </div>

                <canvas id="rewardChart" height="120" class="mt-2"></canvas>
            </div>
        </div>

        <div class="col-lg-4">
            <div class="panel-card h-100">
                <div class="panel-title mb-1">Mastery Mix</div>
                <div class="panel-sub mb-3">Where every flashcard sits in your memory pipeline.</div>

                <div class="donut-wrap">
                    <canvas id="masteryDonut" height="220"></canvas>
                    <div class="donut-center">
                        <div class="v">@(Model.TotalCards == 0 ? 0 : (int)Math.Round((double)Model.CardsMastered * 100 / Model.TotalCards))%</div>
                        <div class="l">Mastered</div>
                    </div>
                </div>

                <div class="mastery-row">
                    <div class="mr-item">
                        <div class="v">@Model.CardsNew</div>
                        <div class="l">New</div>
                    </div>
                    <div class="mr-item">
                        <div class="v">@Model.CardsLearning</div>
                        <div class="l">Learning</div>
                    </div>
                    <div class="mr-item">
                        <div class="v">@Model.CardsMastered</div>
                        <div class="l">Mastered</div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- BLITZ MATRIX + STREAK CALENDAR -->
    <div class="row g-4 mb-4 reveal-up">
        <div class="col-lg-7">
            <div class="panel-card h-100">
                <div class="panel-title mb-1">KanaBlitz Accuracy Matrix</div>
                <div class="panel-sub mb-3">Best accuracy per script &amp; difficulty. Hit 90% to unlock the next tier.</div>

                <div class="blitz-matrix">
                    <div class="bm-head"></div>
                    <div class="bm-head text-center">Hiragana</div>
                    <div class="bm-head text-center">Katakana</div>
                    <div class="bm-head text-center">Dakuten</div>
                    <div class="bm-head text-center">Mixed</div>

                    @foreach (var row in Model.BlitzRows)
                    {
                        <div class="bm-row-head">@row.Difficulty.ToUpper()</div>
                        @foreach (var c in row.Cells)
                        {
                            var pct = (int)Math.Round(c.Accuracy * 100);
                            var tier = pct == 0 ? "empty" : (pct >= 95 ? "tier-elite" : pct >= 90 ? "tier-high" : pct >= 70 ? "tier-mid" : "tier-low");
                            <div class="bm-cell @tier">
                                @if (pct == 0) { <span>—</span> } else { <span>@pct%</span> }
                                @if (pct > 0) { <span class="pct-bar" style="width:@(Math.Min(100, pct))%;"></span> }
                            </div>
                        }
                    }
                </div>
            </div>
        </div>

        <div class="col-lg-5">
            <div class="panel-card h-100">
                <div class="panel-title mb-1">30-Day Activity</div>
                <div class="panel-sub mb-3">Each square = a day of flashcard reviews.</div>

                <div class="heat-calendar" id="heatCalendar"></div>

                <div class="d-flex justify-content-between align-items-center mt-3" style="font-size: 0.7rem; font-weight: 800; color: #64748b; letter-spacing: 0.5px;">
                    <span>Less</span>
                    <div class="d-flex gap-1">
                        <span class="heat-cell" style="width:14px; height:14px;"></span>
                        <span class="heat-cell l1" style="width:14px; height:14px;"></span>
                        <span class="heat-cell l2" style="width:14px; height:14px;"></span>
                        <span class="heat-cell l3" style="width:14px; height:14px;"></span>
                        <span class="heat-cell l4" style="width:14px; height:14px;"></span>
                    </div>
                    <span>More</span>
                </div>
            </div>
        </div>
    </div>

    <!-- INSIGHTS + TOP DECKS -->
    <div class="row g-4 mb-4 reveal-up">
        <div class="col-lg-6">
            <div class="panel-card h-100">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <div class="panel-title mb-0">Productivity Coach</div>
                        <div class="panel-sub">Targeted tips based on your last 30 days.</div>
                    </div>
                    <i class="bi bi-lightning-charge-fill fs-4" style="color: #f59e0b;"></i>
                </div>
                <ul class="insight-list">
                    @foreach (var tip in Model.Insights)
                    {
                        <li><i class="bi bi-stars"></i><span>@tip</span></li>
                    }
                </ul>
            </div>
        </div>

        <div class="col-lg-6">
            <div class="panel-card h-100">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <div class="panel-title mb-0">Top Decks</div>
                        <div class="panel-sub">Your strongest libraries by mastered cards.</div>
                    </div>
                    <i class="bi bi-collection-fill fs-4" style="color: #2563eb;"></i>
                </div>

                @if (Model.TopDecks.Count == 0)
                {
                    <p class="text-muted fw-bold small mb-0">No decks yet. Create one to start tracking mastery.</p>
                }
                else
                {
                    @foreach (var d in Model.TopDecks)
                    {
                        var pct = d.TotalCards == 0 ? 0 : (int)Math.Round((double)d.Mastered * 100 / d.TotalCards);
                        <a class="text-decoration-none" href="@Url.Action("Edit", "Flashcards", new { id = d.Id })">
                            <div class="deck-row">
                                <div class="deck-color" style="background-color: @(string.IsNullOrEmpty(d.ThemeColor) ? "#ffffff" : d.ThemeColor);"></div>
                                <div class="flex-grow-1 min-w-0">
                                    <div class="deck-title text-truncate">@d.Title</div>
                                    <div class="deck-sub">@d.Mastered / @d.TotalCards mastered &middot; @pct%</div>
                                    <div class="mastery-bar"><div style="width:@pct%;"></div></div>
                                </div>
                                <i class="bi bi-chevron-right text-muted"></i>
                            </div>
                        </a>
                    }
                }
            </div>
        </div>
    </div>

</div>

@section Scripts {
<script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script>
<script>
    const DAILY = @Html.Raw(dailyJson);
    const CALENDAR = @Html.Raw(calendarJson);
    const MASTERY = {
        new: @Model.CardsNew,
        learning: @Model.CardsLearning,
        mastered: @Model.CardsMastered
    };
    const WEEKLY  = { exp: @Model.WeeklyExp,  gold: @Model.WeeklyGold };
    const MONTHLY = { exp: @Model.MonthlyExp, gold: @Model.MonthlyGold };

    // ── Reward Chart (XP + Gold over time) ──
    const ctx = document.getElementById('rewardChart').getContext('2d');
    let currentRange = 'week';
    function rangeData(range) {
        const slice = range === 'week' ? DAILY.slice(-7) : DAILY;
        return {
            labels: slice.map(r => r.d),
            xp: slice.map(r => r.exp),
            gold: slice.map(r => r.gold)
        };
    }

    const initial = rangeData('week');
    const rewardChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: initial.labels,
            datasets: [
                {
                    label: 'XP', data: initial.xp,
                    borderColor: '#0ea5e9', backgroundColor: 'rgba(14, 165, 233, 0.15)',
                    tension: 0.35, fill: true, pointRadius: 4, pointBackgroundColor: '#0ea5e9', borderWidth: 3
                },
                {
                    label: 'Gold', data: initial.gold,
                    borderColor: '#f59e0b', backgroundColor: 'rgba(245, 158, 11, 0.12)',
                    tension: 0.35, fill: true, pointRadius: 4, pointBackgroundColor: '#f59e0b', borderWidth: 3
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top', align: 'end',
                    labels: { font: { weight: '900', size: 11 }, color: '#0f172a', usePointStyle: true, pointStyle: 'circle' }
                },
                tooltip: {
                    backgroundColor: '#0f172a', titleColor: '#fbbf24', bodyColor: '#ffffff',
                    titleFont: { weight: '900' }, padding: 12, cornerRadius: 12, displayColors: true
                }
            },
            scales: {
                x: { grid: { display: false }, ticks: { color: '#94a3b8', font: { weight: '700', size: 10 } } },
                y: { grid: { color: '#f1f5f9' }, ticks: { color: '#94a3b8', font: { weight: '700', size: 10 } }, beginAtZero: true }
            }
        }
    });

    document.querySelectorAll('.range-toggle .btn').forEach(b => {
        b.addEventListener('click', () => {
            document.querySelectorAll('.range-toggle .btn').forEach(x => x.classList.remove('active'));
            b.classList.add('active');
            currentRange = b.dataset.range;

            const d = rangeData(currentRange);
            rewardChart.data.labels = d.labels;
            rewardChart.data.datasets[0].data = d.xp;
            rewardChart.data.datasets[1].data = d.gold;
            rewardChart.update();

            const t = currentRange === 'week' ? WEEKLY : MONTHLY;
            document.getElementById('range-exp').textContent  = t.exp.toLocaleString();
            document.getElementById('range-gold').textContent = t.gold.toLocaleString();
            document.getElementById('range-exp-lbl').textContent  = currentRange === 'week' ? 'this week'  : 'this month';
            document.getElementById('range-gold-lbl').textContent = currentRange === 'week' ? 'this week'  : 'this month';
        });
    });

    // ── Mastery Donut ──
    new Chart(document.getElementById('masteryDonut').getContext('2d'), {
        type: 'doughnut',
        data: {
            labels: ['New', 'Learning', 'Mastered'],
            datasets: [{
                data: [MASTERY.new, MASTERY.learning, MASTERY.mastered],
                backgroundColor: ['#cbd5e1', '#fbbf24', '#22c55e'],
                borderColor: '#ffffff',
                borderWidth: 3,
                hoverOffset: 6
            }]
        },
        options: {
            cutout: '70%',
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: '#0f172a', titleColor: '#fbbf24', bodyColor: '#ffffff',
                    titleFont: { weight: '900' }, padding: 12, cornerRadius: 12
                }
            }
        }
    });

    // ── Heat Calendar ──
    const heatRoot = document.getElementById('heatCalendar');
    const max = Math.max(1, ...CALENDAR.map(c => c.n));
    CALENDAR.forEach(c => {
        const cell = document.createElement('div');
        cell.className = 'heat-cell';
        if (c.n > 0) {
            const ratio = c.n / max;
            if (ratio > 0.75) cell.classList.add('l4');
            else if (ratio > 0.5) cell.classList.add('l3');
            else if (ratio > 0.25) cell.classList.add('l2');
            else cell.classList.add('l1');
        }
        cell.title = `${c.d}: ${c.n} review${c.n === 1 ? '' : 's'}`;
        heatRoot.appendChild(cell);
    });

    document.addEventListener('DOMContentLoaded', () => {
        const obs = new IntersectionObserver(es => es.forEach(e => { if (e.isIntersecting) e.target.classList.add('active'); }), { threshold: 0.05 });
        document.querySelectorAll('.reveal-up').forEach(el => obs.observe(el));
    });
</script>
}@using NipponQuest.Controllers
@model NipponQuest.Controllers.StatsController.StatsViewModel
@{
    ViewData["Title"] = "Stats Vault";
    var user = Model.User;
    var dailyJson = System.Text.Json.JsonSerializer.Serialize(
        Model.DailyRewards.Select(r => new {
            d = r.Date.ToString("MM-dd"),
            iso = r.Date.ToString("yyyy-MM-dd"),
            exp = r.Exp,
            gold = r.Gold
        })
    );
    var calendarJson = System.Text.Json.JsonSerializer.Serialize(
        Model.Calendar.Select(c => new { d = c.Date.ToString("yyyy-MM-dd"), n = c.Count })
    );
    var blitzJson = System.Text.Json.JsonSerializer.Serialize(
        Model.BlitzRows.Select(r => new {
            difficulty = r.Difficulty,
            cells = r.Cells.Select(c => new { a = c.Alphabet, acc = Math.Round(c.Accuracy * 100) })
        })
    );
}

<style>
    /* ── Stats Page (KanaBlitz aesthetic) ─────────────────── */
    .reveal-up { opacity: 0; transform: translateY(30px); transition: all 0.6s cubic-bezier(0.16, 1, 0.3, 1); }
    .reveal-up.active { opacity: 1; transform: translateY(0); }

    .stats-hero {
        background: linear-gradient(135deg, #0f172a 0%, #1e293b 60%, #0ea5e9 140%);
        border-radius: 28px;
        color: #ffffff;
        padding: 2rem;
        position: relative;
        overflow: hidden;
        box-shadow: 0 25px 60px -12px rgba(15, 23, 42, 0.35);
    }
    .stats-hero::before {
        content: "";
        position: absolute;
        inset: -40% -10% auto auto;
        width: 320px; height: 320px;
        background: radial-gradient(circle, rgba(245, 158, 11, 0.18) 0%, transparent 70%);
        pointer-events: none;
    }
    .stats-hero .gamertag {
        font-size: clamp(1.6rem, 3vw, 2.2rem);
        font-weight: 900;
        letter-spacing: -1px;
        line-height: 1;
    }
    .stats-hero .level-chip {
        background: #fbbf24; color: #422006;
        font-weight: 900; letter-spacing: 1px;
        padding: 4px 10px; border-radius: 8px;
        font-size: 0.7rem; text-transform: uppercase;
    }
    .stats-hero .hero-meta {
        font-size: 0.75rem; font-weight: 800; letter-spacing: 1px;
        color: rgba(255,255,255,0.7); text-transform: uppercase;
    }
    .stats-hero .hero-stat .val { font-size: 1.6rem; font-weight: 900; line-height: 1; }
    .stats-hero .hero-stat .lbl { font-size: 0.65rem; font-weight: 800; letter-spacing: 1.5px; color: rgba(255,255,255,0.6); text-transform: uppercase; margin-top: 4px; }

    .progress-thin {
        height: 8px; background: rgba(255,255,255,0.15); border-radius: 50px; overflow: hidden;
    }
    .progress-thin > div {
        height: 100%; background: linear-gradient(90deg, #fbbf24, #f59e0b);
        border-radius: 50px;
    }

    .quick-stat {
        background: #ffffff; border: 1px solid #e2e8f0; border-radius: 18px;
        padding: 1.1rem 1.2rem; transition: all 250ms ease;
        height: 100%;
    }
    .quick-stat:hover { transform: translateY(-3px); box-shadow: 0 12px 28px rgba(15, 23, 42, 0.08); border-color: #cbd5e1; }
    .quick-stat .qs-icon {
        width: 40px; height: 40px; border-radius: 12px;
        display: inline-flex; align-items: center; justify-content: center;
        font-size: 1.1rem; margin-bottom: 10px;
    }
    .quick-stat .qs-val { font-size: 1.6rem; font-weight: 900; color: #0f172a; line-height: 1; }
    .quick-stat .qs-lbl { font-size: 0.6rem; font-weight: 900; letter-spacing: 1.5px; color: #94a3b8; text-transform: uppercase; margin-top: 6px; }
    .quick-stat .qs-sub { font-size: 0.7rem; font-weight: 700; color: #64748b; margin-top: 4px; }

    .panel-card {
        background: #ffffff; border: 1px solid #e2e8f0; border-radius: 24px;
        padding: 1.5rem; box-shadow: 0 8px 24px rgba(15, 23, 42, 0.04);
    }
    .panel-card .panel-title {
        font-size: 0.7rem; font-weight: 900; letter-spacing: 2px;
        color: #0f172a; text-transform: uppercase; margin-bottom: 1rem;
    }
    .panel-card .panel-sub { font-size: 0.75rem; color: #64748b; font-weight: 700; }

    .range-toggle .btn {
        background: #f1f5f9; border: none; color: #64748b;
        font-weight: 900; font-size: 0.7rem; letter-spacing: 1.5px; text-transform: uppercase;
        padding: 6px 14px; border-radius: 50px; transition: all 200ms ease;
    }
    .range-toggle .btn.active { background: #0f172a; color: #ffffff; }

    /* Mini reward badges (matches KanaBlitz) */
    .mini-xp {
        width: 22px; height: 22px; border-radius: 6px;
        background: #38bdf8; color: #ffffff;
        display: inline-flex; align-items: center; justify-content: center;
        font-size: 0.55rem; font-weight: 900;
        box-shadow: 0 2px 0 #0284c7;
    }
    .mini-gold {
        width: 22px; height: 22px; border-radius: 50%;
        background: #fbbf24; border: 2px solid #d97706; color: #92400e;
        display: inline-flex; align-items: center; justify-content: center;
        font-size: 0.6rem; font-weight: 900;
        box-shadow: 0 2px 0 #b45309;
    }

    /* Mastery donut center */
    .donut-wrap { position: relative; width: 100%; max-width: 220px; margin: 0 auto; }
    .donut-center {
        position: absolute; inset: 0;
        display: flex; flex-direction: column;
        align-items: center; justify-content: center;
        pointer-events: none;
    }
    .donut-center .v { font-size: 1.8rem; font-weight: 900; color: #0f172a; line-height: 1; }
    .donut-center .l { font-size: 0.6rem; font-weight: 900; letter-spacing: 1.5px; color: #94a3b8; text-transform: uppercase; margin-top: 4px; }

    /* Blitz accuracy matrix */
    .blitz-matrix {
        display: grid;
        grid-template-columns: 110px repeat(4, 1fr);
        gap: 8px;
    }
    .blitz-matrix .bm-head, .blitz-matrix .bm-row-head {
        font-size: 0.62rem; font-weight: 900; letter-spacing: 1.2px;
        color: #94a3b8; text-transform: uppercase;
        display: flex; align-items: center;
    }
    .blitz-matrix .bm-row-head { color: #0f172a; }
    .blitz-matrix .bm-cell {
        background: #f8fafc; border: 1px solid #e2e8f0;
        border-radius: 12px; padding: 14px 10px;
        text-align: center; font-weight: 900; color: #0f172a;
        font-size: 0.95rem;
        position: relative; overflow: hidden;
    }
    .blitz-matrix .bm-cell.empty { color: #cbd5e1; background: #ffffff; }
    .blitz-matrix .bm-cell.tier-low    { background: #fef2f2; border-color: #fecaca; color: #b91c1c; }
    .blitz-matrix .bm-cell.tier-mid    { background: #fffbeb; border-color: #fde68a; color: #b45309; }
    .blitz-matrix .bm-cell.tier-high   { background: #f0fdf4; border-color: #bbf7d0; color: #15803d; }
    .blitz-matrix .bm-cell.tier-elite  { background: linear-gradient(135deg, #ecfdf5 0%, #d1fae5 100%); border-color: #6ee7b7; color: #047857; }
    .blitz-matrix .bm-cell .pct-bar {
        position: absolute; left: 0; right: 0; bottom: 0; height: 3px;
        background: currentColor; opacity: 0.4;
    }

    /* Mastery distribution row */
    .mastery-row { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; margin-top: 14px; }
    .mastery-row .mr-item {
        text-align: center; padding: 12px; border-radius: 14px;
        border: 1px solid #e2e8f0; background: #f8fafc;
    }
    .mastery-row .mr-item .v { font-size: 1.2rem; font-weight: 900; color: #0f172a; }
    .mastery-row .mr-item .l { font-size: 0.6rem; font-weight: 900; letter-spacing: 1px; color: #94a3b8; text-transform: uppercase; margin-top: 2px; }

    /* Insights */
    .insight-list { list-style: none; padding: 0; margin: 0; }
    .insight-list li {
        display: flex; gap: 12px; align-items: flex-start;
        padding: 12px 14px; border-radius: 14px;
        background: #f8fafc; border: 1px solid #e2e8f0;
        margin-bottom: 8px;
        font-size: 0.85rem; font-weight: 700; color: #475569;
    }
    .insight-list li i {
        color: #f59e0b; font-size: 1rem; flex-shrink: 0; margin-top: 2px;
    }

    /* Activity calendar (last 30 days) */
    .heat-calendar {
        display: grid;
        grid-template-columns: repeat(15, minmax(0, 1fr));
        gap: 4px;
    }
    .heat-cell {
        aspect-ratio: 1 / 1;
        border-radius: 6px;
        background: #f1f5f9;
        border: 1px solid #e2e8f0;
    }
    .heat-cell.l1 { background: #dbeafe; border-color: #bfdbfe; }
    .heat-cell.l2 { background: #93c5fd; border-color: #60a5fa; }
    .heat-cell.l3 { background: #3b82f6; border-color: #2563eb; }
    .heat-cell.l4 { background: #1d4ed8; border-color: #1e40af; }

    /* Top decks list */
    .deck-row {
        display: flex; align-items: center; gap: 12px;
        padding: 12px 14px; border-radius: 14px; margin-bottom: 8px;
        border: 1px solid #e2e8f0; background: #ffffff;
        transition: all 200ms ease;
    }
    .deck-row:hover { transform: translateY(-2px); box-shadow: 0 8px 18px rgba(15, 23, 42, 0.06); }
    .deck-row .deck-color {
        width: 36px; height: 36px; border-radius: 10px;
        border: 2px solid #e2e8f0; flex-shrink: 0;
    }
    .deck-row .deck-title { font-weight: 900; color: #0f172a; font-size: 0.9rem; line-height: 1.2; }
    .deck-row .deck-sub { font-size: 0.7rem; color: #64748b; font-weight: 700; }
    .deck-row .mastery-bar {
        height: 6px; background: #e2e8f0; border-radius: 50px; overflow: hidden;
        margin-top: 6px;
    }
    .deck-row .mastery-bar > div {
        height: 100%; background: linear-gradient(90deg, #22c55e, #16a34a);
        border-radius: 50px;
    }

    .info-bubble-btn {
        width: 36px; height: 36px; border-radius: 50%;
        background: #ffffff; border: 2px solid #e2e8f0; color: #64748b;
        display: inline-flex; align-items: center; justify-content: center;
        transition: all 200ms ease;
    }
    .info-bubble-btn:hover { border-color: #0ea5e9; color: #0ea5e9; }
</style>

<div class="container py-5">

    <!-- HERO -->
    <div class="stats-hero mb-4 reveal-up">
        <div class="row g-4 align-items-center">
            <div class="col-md-6">
                <div class="hero-meta mb-2">
                    <i class="bi bi-graph-up-arrow me-1"></i> STATS VAULT
                </div>
                <div class="d-flex align-items-center gap-3 mb-3 flex-wrap">
                    <span class="gamertag">@(string.IsNullOrEmpty(user.GamerTag) ? user.UserName : user.GamerTag)</span>
                    <span class="level-chip">LV @user.Level</span>
                </div>

                <div class="d-flex flex-wrap gap-4 mb-3">
                    <div class="hero-stat">
                        <div class="val">@user.TotalEXP.ToString("N0")</div>
                        <div class="lbl">Total XP</div>
                    </div>
                    <div class="hero-stat">
                        <div class="val">@user.Gold.ToString("N0")</div>
                        <div class="lbl">Gold</div>
                    </div>
                    <div class="hero-stat">
                        <div class="val">@user.LoginStreak</div>
                        <div class="lbl">Day Streak</div>
                    </div>
                    <div class="hero-stat">
                        <div class="val">@user.LessonsCompleted</div>
                        <div class="lbl">Lessons</div>
                    </div>
                </div>

                <div class="hero-meta mb-1">
                    Level Progress
                    <span class="ms-2" style="color:#fbbf24;">@user.CurrentXP / @user.RequiredXP XP</span>
                </div>
                <div class="progress-thin">
                    <div style="width: @(Math.Min(100, user.RequiredXP > 0 ? (user.CurrentXP * 100 / user.RequiredXP) : 0))%;"></div>
                </div>
            </div>

            <div class="col-md-6">
                <div class="row g-3">
                    <div class="col-6">
                        <div class="p-3 rounded-4" style="background: rgba(255,255,255,0.08); border: 1px solid rgba(255,255,255,0.12);">
                            <div class="hero-meta">League</div>
                            <div class="fw-black fs-5 mt-1">@user.CurrentLeague.ToString().ToUpper()</div>
                        </div>
                    </div>
                    <div class="col-6">
                        <div class="p-3 rounded-4" style="background: rgba(255,255,255,0.08); border: 1px solid rgba(255,255,255,0.12);">
                            <div class="hero-meta">Cards Mastered</div>
                            <div class="fw-black fs-5 mt-1">@Model.CardsMastered <span class="opacity-75 fs-6">/ @Model.TotalCards</span></div>
                        </div>
                    </div>
                    <div class="col-6">
                        <div class="p-3 rounded-4" style="background: rgba(255,255,255,0.08); border: 1px solid rgba(255,255,255,0.12);">
                            <div class="hero-meta">Blitz Accuracy</div>
                            <div class="fw-black fs-5 mt-1">@Math.Round(Model.OverallBlitzAccuracy * 100)%</div>
                        </div>
                    </div>
                    <div class="col-6">
                        <div class="p-3 rounded-4" style="background: rgba(255,255,255,0.08); border: 1px solid rgba(255,255,255,0.12);">
                            <div class="hero-meta">Due Today</div>
                            <div class="fw-black fs-5 mt-1">@Model.CardsDueToday</div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- QUICK STATS GRID -->
    <div class="row g-3 mb-4 reveal-up">
        <div class="col-md-2 col-6">
            <div class="quick-stat">
                <div class="qs-icon" style="background:#eff6ff; color:#2563eb;"><i class="bi bi-card-list"></i></div>
                <div class="qs-val">@Model.CardsLearned</div>
                <div class="qs-lbl">Cards Learned</div>
                <div class="qs-sub">of @Model.TotalCards total</div>
            </div>
        </div>
        <div class="col-md-2 col-6">
            <div class="quick-stat">
                <div class="qs-icon" style="background:#f0fdf4; color:#16a34a;"><i class="bi bi-trophy-fill"></i></div>
                <div class="qs-val">@Model.CardsMastered</div>
                <div class="qs-lbl">Mastered</div>
                <div class="qs-sub">5+ correct streaks</div>
            </div>
        </div>
        <div class="col-md-2 col-6">
            <div class="quick-stat">
                <div class="qs-icon" style="background:#fef3c7; color:#b45309;"><i class="bi bi-collection-fill"></i></div>
                <div class="qs-val">@Model.TotalDecks</div>
                <div class="qs-lbl">Total Decks</div>
                <div class="qs-sub">@Model.CommunityClones from community</div>
            </div>
        </div>
        <div class="col-md-2 col-6">
            <div class="quick-stat">
                <div class="qs-icon" style="background:#fff7ed; color:#ea580c;"><i class="bi bi-fire"></i></div>
                <div class="qs-val">@Model.ReviewedToday</div>
                <div class="qs-lbl">Today</div>
                <div class="qs-sub">cards reviewed</div>
            </div>
        </div>
        <div class="col-md-2 col-6">
            <div class="quick-stat">
                <div class="qs-icon" style="background:#fef2f2; color:#dc2626;"><i class="bi bi-bullseye"></i></div>
                <div class="qs-val">@Model.CardsDueToday</div>
                <div class="qs-lbl">Due Now</div>
                <div class="qs-sub">awaiting review</div>
            </div>
        </div>
        <div class="col-md-2 col-6">
            <div class="quick-stat">
                <div class="qs-icon" style="background:#f5f3ff; color:#7c3aed;"><i class="bi bi-megaphone-fill"></i></div>
                <div class="qs-val">@Model.PublishedDecks</div>
                <div class="qs-lbl">Published</div>
                <div class="qs-sub">decks in market</div>
            </div>
        </div>
    </div>

    <!-- ACTIVITY + REWARDS -->
    <div class="row g-4 mb-4 reveal-up">
        <div class="col-lg-8">
            <div class="panel-card h-100">
                <div class="d-flex justify-content-between align-items-start flex-wrap gap-2 mb-2">
                    <div>
                        <div class="panel-title">XP &amp; Gold Earned</div>
                        <div class="panel-sub">Tracking how productivity translates into rewards.</div>
                    </div>
                    <div class="range-toggle d-flex gap-1" role="group">
                        <button type="button" class="btn active" data-range="week">Week</button>
                        <button type="button" class="btn" data-range="month">Month</button>
                    </div>
                </div>

                <div class="d-flex flex-wrap gap-3 mt-3 mb-2">
                    <div class="d-flex align-items-center gap-2">
                        <span class="mini-xp">XP</span>
                        <span class="fw-black" id="range-exp">@Model.WeeklyExp.ToString("N0")</span>
                        <span class="text-muted small fw-bold" id="range-exp-lbl">this week</span>
                    </div>
                    <div class="d-flex align-items-center gap-2">
                        <span class="mini-gold">G</span>
                        <span class="fw-black" id="range-gold">@Model.WeeklyGold.ToString("N0")</span>
                        <span class="text-muted small fw-bold" id="range-gold-lbl">this week</span>
                    </div>
                </div>

                <canvas id="rewardChart" height="120" class="mt-2"></canvas>
            </div>
        </div>

        <div class="col-lg-4">
            <div class="panel-card h-100">
                <div class="panel-title mb-1">Mastery Mix</div>
                <div class="panel-sub mb-3">Where every flashcard sits in your memory pipeline.</div>

                <div class="donut-wrap">
                    <canvas id="masteryDonut" height="220"></canvas>
                    <div class="donut-center">
                        <div class="v">@(Model.TotalCards == 0 ? 0 : (int)Math.Round((double)Model.CardsMastered * 100 / Model.TotalCards))%</div>
                        <div class="l">Mastered</div>
                    </div>
                </div>

                <div class="mastery-row">
                    <div class="mr-item">
                        <div class="v">@Model.CardsNew</div>
                        <div class="l">New</div>
                    </div>
                    <div class="mr-item">
                        <div class="v">@Model.CardsLearning</div>
                        <div class="l">Learning</div>
                    </div>
                    <div class="mr-item">
                        <div class="v">@Model.CardsMastered</div>
                        <div class="l">Mastered</div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- BLITZ MATRIX + STREAK CALENDAR -->
    <div class="row g-4 mb-4 reveal-up">
        <div class="col-lg-7">
            <div class="panel-card h-100">
                <div class="panel-title mb-1">KanaBlitz Accuracy Matrix</div>
                <div class="panel-sub mb-3">Best accuracy per script &amp; difficulty. Hit 90% to unlock the next tier.</div>

                <div class="blitz-matrix">
                    <div class="bm-head"></div>
                    <div class="bm-head text-center">Hiragana</div>
                    <div class="bm-head text-center">Katakana</div>
                    <div class="bm-head text-center">Dakuten</div>
                    <div class="bm-head text-center">Mixed</div>

                    @foreach (var row in Model.BlitzRows)
                    {
                        <div class="bm-row-head">@row.Difficulty.ToUpper()</div>
                        @foreach (var c in row.Cells)
                        {
                            var pct = (int)Math.Round(c.Accuracy * 100);
                            var tier = pct == 0 ? "empty" : (pct >= 95 ? "tier-elite" : pct >= 90 ? "tier-high" : pct >= 70 ? "tier-mid" : "tier-low");
                            <div class="bm-cell @tier">
                                @if (pct == 0) { <span>—</span> } else { <span>@pct%</span> }
                                @if (pct > 0) { <span class="pct-bar" style="width:@(Math.Min(100, pct))%;"></span> }
                            </div>
                        }
                    }
                </div>
            </div>
        </div>

        <div class="col-lg-5">
            <div class="panel-card h-100">
                <div class="panel-title mb-1">30-Day Activity</div>
                <div class="panel-sub mb-3">Each square = a day of flashcard reviews.</div>

                <div class="heat-calendar" id="heatCalendar"></div>

                <div class="d-flex justify-content-between align-items-center mt-3" style="font-size: 0.7rem; font-weight: 800; color: #64748b; letter-spacing: 0.5px;">
                    <span>Less</span>
                    <div class="d-flex gap-1">
                        <span class="heat-cell" style="width:14px; height:14px;"></span>
                        <span class="heat-cell l1" style="width:14px; height:14px;"></span>
                        <span class="heat-cell l2" style="width:14px; height:14px;"></span>
                        <span class="heat-cell l3" style="width:14px; height:14px;"></span>
                        <span class="heat-cell l4" style="width:14px; height:14px;"></span>
                    </div>
                    <span>More</span>
                </div>
            </div>
        </div>
    </div>

    <!-- INSIGHTS + TOP DECKS -->
    <div class="row g-4 mb-4 reveal-up">
        <div class="col-lg-6">
            <div class="panel-card h-100">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <div class="panel-title mb-0">Productivity Coach</div>
                        <div class="panel-sub">Targeted tips based on your last 30 days.</div>
                    </div>
                    <i class="bi bi-lightning-charge-fill fs-4" style="color: #f59e0b;"></i>
                </div>
                <ul class="insight-list">
                    @foreach (var tip in Model.Insights)
                    {
                        <li><i class="bi bi-stars"></i><span>@tip</span></li>
                    }
                </ul>
            </div>
        </div>

        <div class="col-lg-6">
            <div class="panel-card h-100">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <div class="panel-title mb-0">Top Decks</div>
                        <div class="panel-sub">Your strongest libraries by mastered cards.</div>
                    </div>
                    <i class="bi bi-collection-fill fs-4" style="color: #2563eb;"></i>
                </div>

                @if (Model.TopDecks.Count == 0)
                {
                    <p class="text-muted fw-bold small mb-0">No decks yet. Create one to start tracking mastery.</p>
                }
                else
                {
                    @foreach (var d in Model.TopDecks)
                    {
                        var pct = d.TotalCards == 0 ? 0 : (int)Math.Round((double)d.Mastered * 100 / d.TotalCards);
                        <a class="text-decoration-none" href="@Url.Action("Edit", "Flashcards", new { id = d.Id })">
                            <div class="deck-row">
                                <div class="deck-color" style="background-color: @(string.IsNullOrEmpty(d.ThemeColor) ? "#ffffff" : d.ThemeColor);"></div>
                                <div class="flex-grow-1 min-w-0">
                                    <div class="deck-title text-truncate">@d.Title</div>
                                    <div class="deck-sub">@d.Mastered / @d.TotalCards mastered &middot; @pct%</div>
                                    <div class="mastery-bar"><div style="width:@pct%;"></div></div>
                                </div>
                                <i class="bi bi-chevron-right text-muted"></i>
                            </div>
                        </a>
                    }
                }
            </div>
        </div>
    </div>

</div>

@section Scripts {
<script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script>
<script>
    const DAILY = @Html.Raw(dailyJson);
    const CALENDAR = @Html.Raw(calendarJson);
    const MASTERY = {
        new: @Model.CardsNew,
        learning: @Model.CardsLearning,
        mastered: @Model.CardsMastered
    };
    const WEEKLY  = { exp: @Model.WeeklyExp,  gold: @Model.WeeklyGold };
    const MONTHLY = { exp: @Model.MonthlyExp, gold: @Model.MonthlyGold };

    // ── Reward Chart (XP + Gold over time) ──
    const ctx = document.getElementById('rewardChart').getContext('2d');
    let currentRange = 'week';
    function rangeData(range) {
        const slice = range === 'week' ? DAILY.slice(-7) : DAILY;
        return {
            labels: slice.map(r => r.d),
            xp: slice.map(r => r.exp),
            gold: slice.map(r => r.gold)
        };
    }

    const initial = rangeData('week');
    const rewardChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: initial.labels,
            datasets: [
                {
                    label: 'XP', data: initial.xp,
                    borderColor: '#0ea5e9', backgroundColor: 'rgba(14, 165, 233, 0.15)',
                    tension: 0.35, fill: true, pointRadius: 4, pointBackgroundColor: '#0ea5e9', borderWidth: 3
                },
                {
                    label: 'Gold', data: initial.gold,
                    borderColor: '#f59e0b', backgroundColor: 'rgba(245, 158, 11, 0.12)',
                    tension: 0.35, fill: true, pointRadius: 4, pointBackgroundColor: '#f59e0b', borderWidth: 3
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top', align: 'end',
                    labels: { font: { weight: '900', size: 11 }, color: '#0f172a', usePointStyle: true, pointStyle: 'circle' }
                },
                tooltip: {
                    backgroundColor: '#0f172a', titleColor: '#fbbf24', bodyColor: '#ffffff',
                    titleFont: { weight: '900' }, padding: 12, cornerRadius: 12, displayColors: true
                }
            },
            scales: {
                x: { grid: { display: false }, ticks: { color: '#94a3b8', font: { weight: '700', size: 10 } } },
                y: { grid: { color: '#f1f5f9' }, ticks: { color: '#94a3b8', font: { weight: '700', size: 10 } }, beginAtZero: true }
            }
        }
    });

    document.querySelectorAll('.range-toggle .btn').forEach(b => {
        b.addEventListener('click', () => {
            document.querySelectorAll('.range-toggle .btn').forEach(x => x.classList.remove('active'));
            b.classList.add('active');
            currentRange = b.dataset.range;

            const d = rangeData(currentRange);
            rewardChart.data.labels = d.labels;
            rewardChart.data.datasets[0].data = d.xp;
            rewardChart.data.datasets[1].data = d.gold;
            rewardChart.update();

            const t = currentRange === 'week' ? WEEKLY : MONTHLY;
            document.getElementById('range-exp').textContent  = t.exp.toLocaleString();
            document.getElementById('range-gold').textContent = t.gold.toLocaleString();
            document.getElementById('range-exp-lbl').textContent  = currentRange === 'week' ? 'this week'  : 'this month';
            document.getElementById('range-gold-lbl').textContent = currentRange === 'week' ? 'this week'  : 'this month';
        });
    });

    // ── Mastery Donut ──
    new Chart(document.getElementById('masteryDonut').getContext('2d'), {
        type: 'doughnut',
        data: {
            labels: ['New', 'Learning', 'Mastered'],
            datasets: [{
                data: [MASTERY.new, MASTERY.learning, MASTERY.mastered],
                backgroundColor: ['#cbd5e1', '#fbbf24', '#22c55e'],
                borderColor: '#ffffff',
                borderWidth: 3,
                hoverOffset: 6
            }]
        },
        options: {
            cutout: '70%',
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: '#0f172a', titleColor: '#fbbf24', bodyColor: '#ffffff',
                    titleFont: { weight: '900' }, padding: 12, cornerRadius: 12
                }
            }
        }
    });

    // ── Heat Calendar ──
    const heatRoot = document.getElementById('heatCalendar');
    const max = Math.max(1, ...CALENDAR.map(c => c.n));
    CALENDAR.forEach(c => {
        const cell = document.createElement('div');
        cell.className = 'heat-cell';
        if (c.n > 0) {
            const ratio = c.n / max;
            if (ratio > 0.75) cell.classList.add('l4');
            else if (ratio > 0.5) cell.classList.add('l3');
            else if (ratio > 0.25) cell.classList.add('l2');
            else cell.classList.add('l1');
        }
        cell.title = `${c.d}: ${c.n} review${c.n === 1 ? '' : 's'}`;
        heatRoot.appendChild(cell);
    });

    document.addEventListener('DOMContentLoaded', () => {
        const obs = new IntersectionObserver(es => es.forEach(e => { if (e.isIntersecting) e.target.classList.add('active'); }), { threshold: 0.05 });
        document.querySelectorAll('.reveal-up').forEach(el => obs.observe(el));
    });
</script>
}