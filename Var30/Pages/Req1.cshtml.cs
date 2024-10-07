using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Var30.Pages
{
    public class ClientsSubscriptionsModel : PageModel
    {
        private readonly IMongoDatabase _mongoDB;

        public ClientsSubscriptionsModel(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        public List<ClientSubscription> ClientSubscriptions { get; set; } = new List<ClientSubscription>();

        public async Task OnGetAsync()
        {
            var clientsCollection = _mongoDB.GetCollection<BsonDocument>("Clients");
            var subscriptionsCollection = _mongoDB.GetCollection<BsonDocument>("Subscriptions");

            var clients = await clientsCollection.Find(new BsonDocument()).ToListAsync();
            var subscriptions = await subscriptionsCollection.Find(new BsonDocument()).ToListAsync();

            foreach (var client in clients)
            {
                var clientId = client["_id"].AsObjectId;

                var clientSubscriptions = subscriptions.FindAll(sub => sub["client_id"] == clientId);

                ClientSubscriptions.Add(new ClientSubscription
                {
                    ClientName = client["first_name"].AsString + " " + client["last_name"].AsString,
                    Subscriptions = clientSubscriptions.Select(sub => new SubscriptionInfo
                    {
                        SubscriptionId = sub["_id"].AsObjectId,
                        SubscriptionType = sub["subscription_type"].AsString,
                        StartDate = sub["start_date"].AsDateTime,
                        EndDate = sub["end_date"].AsDateTime
                    }).ToList()
                });
            }
        }
    }

    public class ClientSubscription
    {
        public string ClientName { get; set; }
        public List<SubscriptionInfo> Subscriptions { get; set; }
    }

    public class SubscriptionInfo
    {
        public ObjectId SubscriptionId { get; set; }
        public string SubscriptionType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}