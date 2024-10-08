using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Var30.Pages
{
    public class RatesModel : PageModel
    {
        private readonly IMongoDatabase _mongoDB;

        public RatesModel(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        public List<EquipmentRate> EquipmentRates { get; set; }

        public class EquipmentRate
        {
            public string Name { get; set; }
            public int? WeekdayRate { get; set; }
            public int? WeekendRate { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var equipmentCollection = _mongoDB.GetCollection<BsonDocument>("Equipment");

            // Query to retrieve equipment rates
            var equipmentList = await equipmentCollection.Find(new BsonDocument()).ToListAsync();
            EquipmentRates = new List<EquipmentRate>();

            foreach (var equipment in equipmentList)
            {
                EquipmentRates.Add(new EquipmentRate
                {
                    Name = equipment["type"].AsString + " " + equipment["brand"].AsString + " " + equipment["model"].AsString,
                    WeekdayRate = equipment["weekday_rate"].AsInt32,
                    WeekendRate = equipment["weekend_rate"].AsInt32
                });
            }

            return Page();
        }
    }
}
