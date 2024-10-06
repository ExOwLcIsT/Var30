using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Var30.Pages
{
    public class SubscriptionsIssuedModel : PageModel
    {
        IMongoDatabase _mongoDB;
        public SubscriptionsIssuedModel(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }
        public List<DailySubscription> DailySubscriptions { get; set; } = new();
        public List<TypeSubscription> TypeSubscriptions { get; set; } = new();

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                var subscriptionsCollection = _mongoDB.GetCollection<BsonDocument>("Subscriptions");

                var filter = Builders<BsonDocument>.Filter.Gte("StartDate", startDate.Value) &
                             Builders<BsonDocument>.Filter.Lte("StartDate", endDate.Value);
                var subscriptions = await subscriptionsCollection.Find(filter).ToListAsync();

                DailySubscriptions = subscriptions
                    .GroupBy(sub => sub["StartDate"].ToUniversalTime().Date)
                    .Select(g => new DailySubscription
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .ToList();

                TypeSubscriptions = subscriptions
                    .GroupBy(sub => sub["Type"].AsString)
                    .Select(g => new TypeSubscription
                    {
                        Type = g.Key,
                        Count = g.Count()
                    })
                    .ToList();
            }
        }

        public class DailySubscription
        {
            public DateTime Date { get; set; }
            public int Count { get; set; }
        }

        public class TypeSubscription
        {
            public string Type { get; set; }
            public int Count { get; set; }
        }
    }
}
