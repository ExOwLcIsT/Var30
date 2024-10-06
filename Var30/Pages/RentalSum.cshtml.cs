using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Var30.Pages
{
    public class RentalSumModel : PageModel
    {
        public List<RentalSumStats> MonthlyRentalSums { get; set; } = new();
        public List<RentalSumStats> QuarterlyRentalSums { get; set; } = new();

        private readonly IMongoDatabase _mongoDB;

        public RentalSumModel(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
        {
            var rentalsCollection = _mongoDB.GetCollection<BsonDocument>("Rentals");

            // Filter rentals based on the provided period, if given
            var filter = Builders<BsonDocument>.Filter.Empty;
            if (startDate.HasValue && endDate.HasValue)
            {
                filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Gte("RentalDate", startDate.Value),
                    Builders<BsonDocument>.Filter.Lte("RentalDate", endDate.Value)
                );
            }

            // Fetch all rentals
            var rentals = await rentalsCollection.Find(filter).ToListAsync();

            // Group rentals by month and quarter, then calculate the total sum
            MonthlyRentalSums = rentals
                .GroupBy(r => new DateTime(r["RentalDate"].ToUniversalTime().Year, r["RentalDate"].ToUniversalTime().Month, 1)) // Group by year and month
                .Select(g => new RentalSumStats
                {
                    Period = g.Key.ToString("MMMM yyyy"),
                    TotalAmount = g.Sum(r => r["RentalAmount"].AsDecimal)
                })
                .ToList();

            QuarterlyRentalSums = rentals
                .GroupBy(r => new { Year = r["RentalDate"].ToUniversalTime().Year, Quarter = (r["RentalDate"].ToUniversalTime().Month - 1) / 3 + 1 }) // Group by year and quarter
                .Select(g => new RentalSumStats
                {
                    Period = $"Q{g.Key.Quarter} {g.Key.Year}",
                    TotalAmount = g.Sum(r => r["RentalAmount"].AsDecimal)
                })
                .ToList();
        }

        public class RentalSumStats
        {
            public string Period { get; set; }
            public decimal TotalAmount { get; set; }
        }
    }
}
