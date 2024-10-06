using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Var30.Pages
{
    public class SkiEquipmentModel : PageModel
    {
        public List<EquipmentOrder> EquipmentOrders { get; set; } = new List<EquipmentOrder>();
        private readonly IMongoDatabase _mongoDB;
        public SkiEquipmentModel(IMongoDatabase mongoDB) {
            _mongoDB = mongoDB;
        }

        public async Task OnGetAsync()
        {
            var _equipmentCollection = _mongoDB.GetCollection<BsonDocument>("Equipment");
            var equipmentList = await _equipmentCollection.Find(_ => true).ToListAsync();
            var sortedEquipment = equipmentList
                .OrderByDescending(eq => eq["OrdersInWeek"].AsInt32) 
                .Take(10) 
                .Select(eq => new EquipmentOrder
                {
                    Type = eq["Type"].ToString(),
                    OrderCount = eq["OrdersInWeek"].AsInt32
                })
                .ToList();
            EquipmentOrders = sortedEquipment;
        }

        public class EquipmentOrder
        {
            public string Type { get; set; }
            public int OrderCount { get; set; }
        }
    }
}
