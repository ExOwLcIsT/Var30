using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
namespace Var30.Pages
{
    public class ClientsVisitsModel : PageModel
    {
        private readonly IMongoDatabase _mongoDB;

        public ClientsVisitsModel(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        public List<ClientVisitInfo> ClientsVisitedInRange { get; set; } = new List<ClientVisitInfo>();
        public List<ClientVisitInfo> FrequentVisitors { get; set; } = new List<ClientVisitInfo>();

        public async Task<IActionResult> OnGetAsync()
        {
            var liftUsageCollection = _mongoDB.GetCollection<BsonDocument>("LiftUsage");
            var clientsCollection = _mongoDB.GetCollection<BsonDocument>("Clients");

            // Retrieve all lift usage records
            var liftUsageResults = await liftUsageCollection.Find(new BsonDocument()).ToListAsync();
            var clients = await clientsCollection.Find(new BsonDocument()).ToListAsync();

            // Define the date range
            var startDate = new DateTime(2024, 1, 2);
            var endDate = new DateTime(2024, 1, 12);

            // Filter lift usage records for the date range using C#
            var clientsInDateRange = liftUsageResults
                .Where(x => x["usage_date"].ToUniversalTime() >= startDate && x["usage_date"].ToUniversalTime() <= endDate)
                .GroupBy(x => x["client_id"].AsObjectId)
                .Select(g => new
                {
                    ClientId = g.Key,
                    VisitCount = g.Count(),
                    FirstVisitDate = g.Min(x => x["usage_date"].ToUniversalTime()),
                    LastVisitDate = g.Max(x => x["usage_date"].ToUniversalTime())
                })
                .ToList();

            // Add clients who visited within the date range
            foreach (var visit in clientsInDateRange)
            {
                var client = clients.FirstOrDefault(c => c["_id"].AsObjectId == visit.ClientId);
                if (client != null)
                {
                    ClientsVisitedInRange.Add(new ClientVisitInfo
                    {
                        Name = client["first_name"].AsString + " "+ client["last_name"].AsString,
                        VisitCount = visit.VisitCount,
                        FirstVisitDate = visit.FirstVisitDate,
                        LastVisitDate = visit.LastVisitDate
                    });
                }
            }

            // Filter for clients who visited more than 3 times
            var frequentVisitorsData = liftUsageResults
                .GroupBy(x => x["client_id"].AsObjectId)
                .Where(g => g.Count() > 3)
                .Select(g => new
                {
                    ClientId = g.Key,
                    VisitCount = g.Count()
                })
                .ToList();

            // Add frequent visitors' details
            foreach (var visit in frequentVisitorsData)
            {
                var client = clients.FirstOrDefault(c => c["_id"].AsObjectId == visit.ClientId);
                if (client != null)
                {
                    FrequentVisitors.Add(new ClientVisitInfo
                    {
                        Name = client["name"].AsString,
                        VisitCount = visit.VisitCount,
                        FirstVisitDate = null, // No applicable date in this context
                        LastVisitDate = null // No applicable date in this context
                    });
                }
            }

            return Page();
        }
    }
    public class ClientVisitInfo
    {
        public string Name { get; set; }
        public int VisitCount { get; set; }
        public DateTime? FirstVisitDate { get; set; } // Make this nullable
        public DateTime? LastVisitDate { get; set; }  // Make this nullable
    }
}
