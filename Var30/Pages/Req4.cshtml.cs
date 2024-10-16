using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Var30.Pages
{
    public class LiftUsageStatsModel : PageModel
    {
        private readonly IMongoDatabase _mongoDB;

        public LiftUsageStatsModel(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }

        public Dictionary<string, int> DailyUsage { get; set; }
        public Dictionary<string, int> WeeklyUsage { get; set; }
        public Dictionary<string, int> MonthlyUsage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var liftUsageCollection = _mongoDB.GetCollection<BsonDocument>("LiftUsage");
            var liftsCollection = _mongoDB.GetCollection<BsonDocument>("Lifts");

            // Fetch lifts and map their IDs to names
            var lifts = await liftsCollection.Find(new BsonDocument()).ToListAsync();
            var liftNames = new Dictionary<ObjectId, string>();
            foreach (var lift in lifts)
            {
                liftNames[lift["_id"].AsObjectId] = lift["name"].AsString;
            }

            // Daily usage
            DailyUsage = await GetLiftUsageCount(liftUsageCollection, TimeSpan.FromDays(1), liftNames);

            // Weekly usage
            WeeklyUsage = await GetLiftUsageCount(liftUsageCollection, TimeSpan.FromDays(7), liftNames);

            // Monthly usage
            MonthlyUsage = await GetLiftUsageCount(liftUsageCollection, TimeSpan.FromDays(30), liftNames);

            return Page();
        }

        private async Task<Dictionary<string, int>> GetLiftUsageCount(
            IMongoCollection<BsonDocument> collection,
            TimeSpan timeSpan,
            Dictionary<ObjectId, string> liftNames)
        {
            var usageCount = new Dictionary<string, int>();

            var now = DateTime.UtcNow;
            var cutoffDate = now - timeSpan;

            // Fetch documents and filter in memory
            var liftUsageDocuments = await collection.Find(new BsonDocument()).ToListAsync();

            foreach (var doc in liftUsageDocuments)
            {
                var usageDate = doc["usage_date"].ToUniversalTime(); // Convert to UTC for comparison
                if (usageDate < cutoffDate) continue; // Skip if outside the range

                var liftId = doc["lift_id"].AsObjectId;

                // Get lift name
                if (liftNames.TryGetValue(liftId, out var liftName))
                {
                    if (usageCount.ContainsKey(liftName))
                    {
                        usageCount[liftName]++;
                    }
                    else
                    {
                        usageCount[liftName] = 1;
                    }
                }
            }

            return usageCount;
        }
    }
}
