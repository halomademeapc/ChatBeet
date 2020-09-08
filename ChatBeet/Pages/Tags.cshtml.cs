using ChatBeet.Data;
using ChatBeet.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBeet.Pages
{
    public class TagsModel : PageModel
    {
        private readonly BooruContext db;
        private readonly IMemoryCache cache;
        private static readonly Random rng = new Random();

        public IEnumerable<Stat> GeneralStats { get; private set; }
        public IEnumerable<Stat> RandomStats { get; private set; }
        public IEnumerable<TopTag> UserStats { get; private set; }
        public DateTime Earliest { get; private set; }

        public TagsModel(BooruContext db, IMemoryCache cache)
        {
            this.db = db;
            this.cache = cache;
        }

        public async Task OnGet()
        {
            GeneralStats = (await GetStats())
                .OrderByDescending(s => s.Total)
                .Take(20)
                .ToList();
            RandomStats = (await GetStats())
                .OrderBy(s => rng.Next())
                .Take(20)
                .ToList();
            UserStats = await cache.GetOrCreate("tags:user", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return await db.GetTopTags();
            });
            Earliest = await cache.GetOrCreate("tags:date", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return await EntityFrameworkQueryableExtensions.MinAsync(db.TagHistories, th => th.Timestamp);
            });
        }

        private async Task<IEnumerable<Stat>> GetStats() => await cache.GetOrCreateAsync("tags:stats", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return await db.TagHistories.AsQueryable()
                .GroupBy(th => th.Tag)
                .Select(g => new Stat { Tag = g.Key, Total = g.Count() })
                .ToListAsync();
        });

        public struct Stat
        {
            public string Tag;
            public int Total;
        }
    }
}
