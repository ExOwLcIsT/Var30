using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Var30.Pages
{
    public class ExhaustedLiftsModel : PageModel
    {
        public List<ClientLiftInfo> ClientsWithExhaustedLifts { get; set; } = new();
        public List<ClientLiftInfo> ClientsWithMoreThan15Lifts { get; set; } = new();
        public DateTime? specifiedDate { get; set; }

        private readonly IMongoDatabase _mongoDB;

        public ExhaustedLiftsModel(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        public async Task OnGetAsync(DateTime? specifiedDate)
        {
            this.specifiedDate = specifiedDate;

            var clientsCollection = _mongoDB.GetCollection<BsonDocument>("Clients");
            var subscriptionsCollection = _mongoDB.GetCollection<BsonDocument>("Subscriptions");
            var liftsCollection = _mongoDB.GetCollection<BsonDocument>("Lifts");

            // Filter for lifts on the specified date, if provided
            var liftFilter = Builders<BsonDocument>.Filter.Empty;
            if (specifiedDate.HasValue)
            {
                liftFilter = Builders<BsonDocument>.Filter.Gte("LiftDate", specifiedDate.Value.Date) &
                             Builders<BsonDocument>.Filter.Lt("LiftDate", specifiedDate.Value.Date.AddDays(1));
            }

            // Fetch all lifts and subscriptions
            var allLifts = await liftsCollection.Find(liftFilter).ToListAsync();
            var allSubscriptions = await subscriptionsCollection.Find(Builders<BsonDocument>.Filter.Empty).ToListAsync();

            // Dictionary to track how many lifts each client has taken
            var clientLiftCounts = allLifts.GroupBy(lift => lift["ClientId"].AsString)
                .ToDictionary(g => g.Key, g => g.Count());

            // Find clients who have exhausted their lifts
            ClientsWithExhaustedLifts = allSubscriptions
                .Where(sub => sub["TotalLiftsAllowed"].AsInt32 > 0 && clientLiftCounts.ContainsKey(sub["ClientId"].AsString))
                .Where(sub => clientLiftCounts[sub["ClientId"].AsString] >= sub["TotalLiftsAllowed"].AsInt32)
                .Select(sub => new ClientLiftInfo
                {
                    ClientId = sub["ClientId"].AsString,
                    ClientName = sub["ClientName"].AsString,
                    TotalLiftsTaken = clientLiftCounts[sub["ClientId"].AsString],
                    TotalLiftsAllowed = sub["TotalLiftsAllowed"].AsInt32
                })
                .ToList();

            // Find clients who took more than 15 lifts on the specified date
            ClientsWithMoreThan15Lifts = allLifts
                .Where(lift => lift["LiftDate"].ToUniversalTime().Date == specifiedDate.Value.Date) // Ensure it filters lifts on the specified date
                .GroupBy(lift => lift["ClientId"].AsString)
                .Where(group => group.Count() > 15)
                .Select(group => new ClientLiftInfo
                {
                    ClientId = group.Key,
                    ClientName = clientsCollection.Find(Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(group.Key)))
                                                 .FirstOrDefault()?["Name"].AsString ?? "Unknown",
                    TotalLiftsTaken = group.Count()
                })
                .ToList();
        }

        public class ClientLiftInfo
        {
            public string ClientId { get; set; }
            public string ClientName { get; set; }
            public int TotalLiftsTaken { get; set; }
            public int TotalLiftsAllowed { get; set; }
        }
    }
}
