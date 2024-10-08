using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Var30.Pages
{
    public class UnlimitedSubscriptionDiscountModel : PageModel
    {
        private readonly IMongoDatabase _mongoDB;

        public UnlimitedSubscriptionDiscountModel(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        public List<ClientDiscountInfo> ClientsWithDiscount { get; set; }

        public class ClientDiscountInfo
        {
            public string ClientName { get; set; }
            public string ClientId { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var subscriptionsCollection = _mongoDB.GetCollection<BsonDocument>("Subscriptions");
            var clientsCollection = _mongoDB.GetCollection<BsonDocument>("Clients");

            // Define the start and end dates for February of the current year
            var startDate = new DateTime(DateTime.Now.Year, 2, 1);
            var endDate = new DateTime(DateTime.Now.Year, 3, 1);

            // Query for unlimited subscriptions in February
            var unlimitedSubscriptions = await subscriptionsCollection.Find(new BsonDocument
            {
                { "subscription_type", "unlimited" },
                { "start_date", new BsonDocument { { "$gte", startDate }, { "$lt", endDate } } }
            }).ToListAsync();

            ClientsWithDiscount = new List<ClientDiscountInfo>();

            foreach (var subscription in unlimitedSubscriptions)
            {
                var clientId = subscription["client_id"].AsObjectId;

                // Retrieve client information
                var client = await clientsCollection.Find(new BsonDocument("_id", clientId)).FirstOrDefaultAsync();

                if (client != null)
                {
                    ClientsWithDiscount.Add(new ClientDiscountInfo
                    {
                        ClientName = client["first_name"].AsString + " " + client["last_name"].AsString,
                        ClientId = clientId.ToString()
                    });
                }
            }

            return Page();
        }
    }
}
