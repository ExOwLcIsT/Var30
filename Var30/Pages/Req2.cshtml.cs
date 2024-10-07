using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SkiEquipmentOrdersModel : PageModel
{
    private readonly IMongoDatabase _mongoDB;

    public SkiEquipmentOrdersModel(IMongoDatabase mongoDB)
    {
        _mongoDB = mongoDB;
    }

    // Властивість для зберігання інформації про замовлене обладнання
    public List<SkiEquipmentOrder> SkiEquipmentOrders { get; set; } = new List<SkiEquipmentOrder>();

    public async Task OnGetAsync()
    {
        var equipmentCollection = _mongoDB.GetCollection<BsonDocument>("Equipment");
        var usageCollection = _mongoDB.GetCollection<BsonDocument>("EquipmentUsage");

        // Отримуємо всі записи використання обладнання за останній тиждень
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var equipmentUsage = await usageCollection.Find(Builders<BsonDocument>.Filter.Gte("date", weekAgo)).ToListAsync();

        // Групуємо дані по обладнанню
        var groupedUsage = equipmentUsage.GroupBy(e => e["equipment_id"])
            .Select(g => new SkiEquipmentOrder
            {
                EquipmentId = g.Key.AsObjectId,
                TotalOrders = g.Count(),
                DailyCounts = g.GroupBy(u => u["date"].ToUniversalTime().Date)
                               .Select(dg => new DailyCount
                               {
                                   Date = dg.Key,
                                   Count = dg.Count()
                               }).ToList()
            }).OrderByDescending(o => o.TotalOrders).ToList();

        // Отримуємо інформацію про обладнання
        foreach (var equipment in groupedUsage)
        {
            var equipmentDoc = await equipmentCollection.Find(Builders<BsonDocument>.Filter.Eq("_id", equipment.EquipmentId)).FirstOrDefaultAsync();
            SkiEquipmentOrders.Add(new SkiEquipmentOrder
            {
                EquipmentId = equipment.EquipmentId,
                EquipmentName = equipmentDoc?["type"].AsString + " " + equipmentDoc?["brand"].AsString + " " + equipmentDoc?["model"].AsString,
                TotalOrders = equipment.TotalOrders,
                DailyCounts = equipment.DailyCounts
            });
        }
    }
}

// Класи для зберігання даних про замовлене обладнання
public class SkiEquipmentOrder
{
    public ObjectId EquipmentId { get; set; }
    public string EquipmentName { get; set; }
    public int TotalOrders { get; set; }
    public List<DailyCount> DailyCounts { get; set; }
}

public class DailyCount
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}
