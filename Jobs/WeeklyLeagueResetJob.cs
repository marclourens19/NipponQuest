using Quartz;
using NipponQuest.Models;
using NipponQuest.Data;
using Microsoft.EntityFrameworkCore;

namespace NipponQuest.Jobs
{
    [DisallowConcurrentExecution]
    public class WeeklyLeagueResetJob : IJob
    {
        private readonly ApplicationDbContext _context;

        // Constructor must correctly inject the DbContext
        public WeeklyLeagueResetJob(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // IMPORTANT: Add a breakpoint or a console log here to see if it fires!
            Console.WriteLine("---> QUARTZ JOB RUNNING: " + DateTime.Now);

            var allUsers = await _context.Users.ToListAsync();

            foreach (var user in allUsers)
            {
                int rewardAmount = GetWeeklyGold(user.CurrentLeague);
                user.Gold += rewardAmount;
                user.WeeklyXP = 0;
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("---> QUARTZ JOB SUCCESSFUL");
        }

        private int GetWeeklyGold(LeagueRank rank) => rank switch
        {
            LeagueRank.Legend => 425,
            LeagueRank.Dragon => 300,
            LeagueRank.Challenger => 225,
            LeagueRank.Master => 175,
            _ => ((int)rank) * 25 + 25
        };
    }
}