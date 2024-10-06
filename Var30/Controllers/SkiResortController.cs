using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Var30
{
    [ApiController]
    [Route("api/[controller]")]
    public class SkiResortController : ControllerBase
    {
        private readonly IMongoCollection<BsonDocument> _clientsCollection;
        private readonly IMongoCollection<BsonDocument> _subscriptionsCollection;
        private readonly IMongoCollection<BsonDocument> _equipmentCollection;
        private readonly IMongoCollection<BsonDocument> _liftsCollection;
        private readonly IMongoCollection<BsonDocument> _workersCollection;

        public SkiResortController(IMongoDatabase database)
        {
            _clientsCollection = database.GetCollection<BsonDocument>("Clients");
            _subscriptionsCollection = database.GetCollection<BsonDocument>("Subscriptions");
            _equipmentCollection = database.GetCollection<BsonDocument>("Equipment");
            _liftsCollection = database.GetCollection<BsonDocument>("Lifts");
            _workersCollection = database.GetCollection<BsonDocument>("Workers");
        }

        // 1. Вивести інформацію про клієнтів та абонементи
        [HttpGet("clients-subscriptions")]
        public async Task<IActionResult> GetClientsAndSubscriptions()
        {
            var clients = await _clientsCollection.Find(_ => true).ToListAsync();
            var subscriptions = await _subscriptionsCollection.Find(_ => true).ToListAsync();

            var results = clients.Select(client =>
            {
                var clientSubscriptions = subscriptions
                    .Where(sub => sub["_id"] == client["SubscriptionId"]) 
                    .Select(sub => sub["Type"].ToString())
                    .ToList();

                return new
                {
                    ClientName = client["Name"].ToString(),
                    Subscriptions = clientSubscriptions.Any() ? string.Join(", ", clientSubscriptions) : "Немає"
                };
            }).ToList();

            return Ok(results);
        }

        // 2. Вивести перелік гірськолижного обладнання, яке замовляли найбільшу кількість разів за тиждень
        [HttpGet("most-ordered-equipment")]
        public async Task<IActionResult> GetMostOrderedEquipment()
        {
            var equipmentList = await _equipmentCollection.Find(_ => true).ToListAsync();
            var sortedEquipment = equipmentList
                .OrderByDescending(eq => eq["OrdersInWeek"].AsInt32) // Sorting equipment by OrdersInWeek
                .Take(10) // Get top 10
                .Select(eq => new
                {
                    Type = eq["Type"].ToString(),
                    Orders = eq["OrdersInWeek"].AsInt32
                })
                .ToList();

            return Ok(sortedEquipment);
        }

        // 3. Визначити скільки абонементів було видано кожного дня протягом вказаного періоду
        [HttpGet("subscriptions-per-day")]
        public async Task<IActionResult> GetSubscriptionsCountByDay([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var subscriptions = await _subscriptionsCollection
                .Find(sub => sub["Date"] >= startDate && sub["Date"] <= endDate)
                .ToListAsync();

            var subscriptionsCountByDay = subscriptions
                .GroupBy(sub => sub["Date"].ToUniversalTime().Date)
                .Select(group => new
                {
                    Date = group.Key.ToShortDateString(),
                    Count = group.Count()
                })
                .ToList();

            return Ok(subscriptionsCountByDay);
        }
    }
}