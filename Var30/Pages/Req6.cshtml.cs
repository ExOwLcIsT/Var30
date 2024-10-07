using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Var30.Pages
{
    public class ClientLiftUsageModel : PageModel
    {
        private readonly IMongoDatabase _mongoDB;

        public ClientLiftUsageModel(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        [BindProperty]
        public DateTime SelectedDate { get; set; }

        public List<ClientUsageInfo> ClientsExceededLiftCount { get; set; } = new List<ClientUsageInfo>();
        public List<ClientUsageInfo> ClientsExceeding15Rides { get; set; } = new List<ClientUsageInfo>();

        public class ClientUsageInfo
        {
            public string ClientName { get; set; }
            public string ClientId { get; set; }
            public int TotalRides { get; set; }
            public int AllowedRides { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var subscriptionsCollection = _mongoDB.GetCollection<BsonDocument>("Subscriptions");
            var liftUsageCollection = _mongoDB.GetCollection<BsonDocument>("LiftUsage");
            var transactionsCollection = _mongoDB.GetCollection<BsonDocument>("Transactions");
            var clientsCollection = _mongoDB.GetCollection<BsonDocument>("Clients"); // Add this line

            // Get all clients and their subscriptions
            var clientSubscriptions = await subscriptionsCollection.Find(new BsonDocument()).ToListAsync();
            var clientIds = clientSubscriptions.Select(sub => sub["client_id"].AsObjectId).Distinct().ToList();

            // Get client names
            var clientNames = await clientsCollection.Find(new BsonDocument
    {
        { "_id", new BsonDocument("$in", new BsonArray(clientIds)) }
    }).ToListAsync();

            var clientNameDict = clientNames.ToDictionary(client => client["_id"].AsObjectId, client => client["first_name"].AsString + " " + client["last_name"].AsString);

            // Get lift usage for the specified date
            var startDate = SelectedDate.Date;
            var endDate = startDate.AddDays(1);

            var liftUsageResults = await liftUsageCollection.Find(new BsonDocument
    {
        { "$and", new BsonArray
            {
                new BsonDocument("client_id", new BsonDocument("$in", new BsonArray(clientIds))),
                new BsonDocument("usage_date", new BsonDocument("$gte", startDate)),
                new BsonDocument("usage_date", new BsonDocument("$lt", endDate))
            }
        }
    }).ToListAsync();

            // Count rides and check against allowed rides
            foreach (var clientSub in clientSubscriptions)
            {
                var clientId = clientSub["client_id"].AsObjectId;
                var allowedRides = 0; // Default value if not found

                // Assuming you get allowed rides from transactions or another logic
                var clientTransactions = await transactionsCollection.Find(new BsonDocument("client_id", clientId)).ToListAsync();
                foreach (var transaction in clientTransactions)
                {
                    // Here, you might define your logic to set allowedRides from transaction data.
                    // allowedRides = DetermineFromTransaction(transaction); // Placeholder
                }

                var totalRides = liftUsageResults.Count(usage => usage["client_id"].AsObjectId == clientId);

                // Get the client's name from the dictionary
                string clientName = clientNameDict.ContainsKey(clientId) ? clientNameDict[clientId] : "Unknown";

                // Check if client exceeded their allowed rides
                if (totalRides > allowedRides)
                {
                    ClientsExceededLiftCount.Add(new ClientUsageInfo
                    {
                        ClientName = clientName, // Set the client's name
                        ClientId = clientId.ToString(),
                        TotalRides = totalRides,
                        AllowedRides = allowedRides
                    });
                }

                // Check if the client has done more than 15 rides
                if (totalRides > 15)
                {
                    ClientsExceeding15Rides.Add(new ClientUsageInfo
                    {
                        ClientName = clientName, // Set the client's name
                        ClientId = clientId.ToString(),
                        TotalRides = totalRides,
                        AllowedRides = allowedRides
                    });
                }
            }

            return Page();
        }

    }
}
