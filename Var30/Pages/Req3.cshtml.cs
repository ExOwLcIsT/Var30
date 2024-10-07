using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Var30.Pages
{
    public class SubscriptionStatsModel : PageModel
    {
        private readonly IMongoDatabase _mongoDB;

        public SubscriptionStatsModel(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }

        public List<DailySubscription> DailySubscriptions { get; set; }
        public Dictionary<string, int> SubscriptionTypeCounts { get; set; }

        public class DailySubscription
        {
            public DateTime Date { get; set; }
            public int Count { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (StartDate > EndDate)
            {
                ModelState.AddModelError(string.Empty, "End date must be greater than start date.");
                return Page();
            }

            var subscriptionsCollection = _mongoDB.GetCollection<BsonDocument>("Subscriptions");

            // Отримати всі документи з колекції
            var allSubscriptions = await subscriptionsCollection.Find(new BsonDocument()).ToListAsync();

            // Фільтрування за датою
            DailySubscriptions = new List<DailySubscription>();
            SubscriptionTypeCounts = new Dictionary<string, int>();

            // Фільтруємо підписки за датою
            foreach (var subscription in allSubscriptions)
            {
                var startDate = subscription["start_date"].ToUniversalTime(); // Отримання дати старту

                // Перевірка, чи поточна дата входить в заданий діапазон
                if (startDate.Date >= StartDate.Date && startDate.Date <= EndDate.Date)
                {
                    // Додаємо до DailySubscriptions
                    var dailySubscription = DailySubscriptions.Find(ds => ds.Date.Date == startDate.Date);
                    if (dailySubscription == null)
                    {
                        DailySubscriptions.Add(new DailySubscription { Date = startDate.Date, Count = 1 });
                    }
                    else
                    {
                        dailySubscription.Count++;
                    }

                    // Додаємо до SubscriptionTypeCounts
                    var subscriptionType = subscription["subscription_type"].AsString;
                    if (SubscriptionTypeCounts.ContainsKey(subscriptionType))
                    {
                        SubscriptionTypeCounts[subscriptionType]++;
                    }
                    else
                    {
                        SubscriptionTypeCounts[subscriptionType] = 1;
                    }
                }
            }

            return Page();
        }
    }
}