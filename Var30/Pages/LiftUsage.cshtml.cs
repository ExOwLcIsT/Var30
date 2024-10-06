using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Var30.Pages
{
    public class LiftUsageModel : PageModel
    {

        public List<LiftUsageStats> LiftUsageByDay { get; set; } = new();
        public List<LiftUsageStats> LiftUsageByWeek { get; set; } = new();
        public List<LiftUsageStats> LiftUsageByMonth { get; set; } = new();

        private readonly IMongoDatabase _mongoDB;

        public LiftUsageModel(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
        {
            var liftsCollection = _mongoDB.GetCollection<BsonDocument>("Lifts");

            // Fetch all lift usage data
            var liftUsage = await liftsCollection.Find(_ => true).ToListAsync();

            // Process lift usage statistics
            LiftUsageByDay = liftUsage
                .Select(lift => new LiftUsageStats
                {
                    LiftName = lift["LiftName"].AsString,
                    ClientsPerDay = lift["ClientsPerDay"].AsInt32,
                    ClientsPerWeek = lift["ClientsPerWeek"].AsInt32,
                    ClientsPerMonth = lift["ClientsPerMonth"].AsInt32
                })
                .OrderByDescending(lift => lift.ClientsPerDay)
                .ToList();

            LiftUsageByWeek = liftUsage
                .Select(lift => new LiftUsageStats
                {
                    LiftName = lift["LiftName"].AsString,
                    ClientsPerDay = lift["ClientsPerDay"].AsInt32,
                    ClientsPerWeek = lift["ClientsPerWeek"].AsInt32,
                    ClientsPerMonth = lift["ClientsPerMonth"].AsInt32
                })
                .OrderByDescending(lift => lift.ClientsPerWeek)
                .ToList();

            LiftUsageByMonth = liftUsage
                .Select(lift => new LiftUsageStats
                {
                    LiftName = lift["LiftName"].AsString,
                    ClientsPerDay = lift["ClientsPerDay"].AsInt32,
                    ClientsPerWeek = lift["ClientsPerWeek"].AsInt32,
                    ClientsPerMonth = lift["ClientsPerMonth"].AsInt32
                })
                .OrderByDescending(lift => lift.ClientsPerMonth)
                .ToList();
        }

        public class LiftUsageStats
        {
            public string LiftName { get; set; }
            public int ClientsPerDay { get; set; }
            public int ClientsPerWeek { get; set; }
            public int ClientsPerMonth { get; set; }
        }
    }
}
