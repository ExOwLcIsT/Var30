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
    public class EquipmentRentalStatsModel : PageModel
    {
        private readonly IMongoDatabase _mongoDB;

        public EquipmentRentalStatsModel(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        public Dictionary<string, decimal> MonthlyTotals { get; set; }
        public Dictionary<string, decimal> QuarterlyTotals { get; set; }

        public async Task OnGetAsync()
        {
            var usageCollection = _mongoDB.GetCollection<BsonDocument>("EquipmentUsage");

            MonthlyTotals = await GetRentalTotals(usageCollection, TimeSpan.FromDays(30), "month");
            QuarterlyTotals = await GetRentalTotals(usageCollection, TimeSpan.FromDays(90), "quarter");
        }

        private async Task<Dictionary<string, decimal>> GetRentalTotals(
            IMongoCollection<BsonDocument> collection,
            TimeSpan timeSpan,
            string period)
        {
            var rentalTotals = new Dictionary<string, decimal>();

            // Fetch all usage documents
            var usageDocuments = await collection.Find(new BsonDocument()).ToListAsync();

            foreach (var doc in usageDocuments)
            {
                var usageDate = doc["date"].ToUniversalTime(); // Convert to UTC
                var hoursUsed = doc["hours_used"].AsInt32;

                // Determine the period key
                string key;
                if (period == "month")
                {
                    key = usageDate.ToString("yyyy-MM"); // Format for month
                }
                else // period == "quarter"
                {
                    int quarter = (usageDate.Month - 1) / 3 + 1;
                    key = $"{usageDate.Year}-Q{quarter}"; // Format for quarter
                }

                // Calculate rental amount (assuming a rate per hour)
                decimal rentalAmount = CalculateRentalAmount(hoursUsed); // Adjust this function as per your rate logic

                if (rentalTotals.ContainsKey(key))
                {
                    rentalTotals[key] += rentalAmount;
                }
                else
                {
                    rentalTotals[key] = rentalAmount;
                }
            }

            return rentalTotals;
        }

        private decimal CalculateRentalAmount(int hoursUsed)
        {
            // Example rate: $10 per hour
            decimal ratePerHour = 10m;
            return hoursUsed * ratePerHour;
        }
    }
}
