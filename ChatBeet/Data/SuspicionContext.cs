﻿using ChatBeet.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBeet.Data
{
    public class SuspicionContext : DbContext
    {
        public SuspicionContext(DbContextOptions<SuspicionContext> opts) : base(opts) { }

        public virtual DbSet<Suspicion> Suspicions { get; set; }

        public Task<int> GetSuspicionLevelAsync(string suspect) => Suspicions
            .AsQueryable()
            .CountAsync(s => s.Suspect.ToLower() == suspect.ToLower());

        public async Task<bool> HasRecentlyReportedAsync(string suspect, string reporter, TimeSpan debounceWindow = default)
        {
            if (debounceWindow == default)
                debounceWindow = TimeSpan.FromMinutes(2);

            var lastReport = await Suspicions
                .AsQueryable()
                .Where(s => s.Reporter.ToLower() == reporter.ToLower())
                .Where(s => s.Suspect.ToLower() == suspect.ToLower())
                .OrderByDescending(s => s.TimeReported)
                .FirstOrDefaultAsync();

            return lastReport != default && (DateTime.Now - lastReport.TimeReported) < debounceWindow;
        }

        public async Task ReportSuspiciousActivityAsync(string suspect, string reporter, bool bypassDebounceCheck = false)
        {
            if (bypassDebounceCheck || !await HasRecentlyReportedAsync(suspect, reporter))
            {
                Suspicions.Add(new Suspicion
                {
                    Reporter = reporter.Trim(),
                    Suspect = suspect.Trim(),
                    TimeReported = DateTime.Now
                });
                await SaveChangesAsync();
            }
        }
    }
}
