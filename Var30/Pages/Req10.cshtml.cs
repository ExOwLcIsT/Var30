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
    public class WorkersAndEquipmentModel : PageModel
    {
        private readonly IMongoDatabase _mongoDB;

        public WorkersAndEquipmentModel(IMongoDatabase mongoDB)
        {
            _mongoDB = mongoDB;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime? SelectedDate { get; set; }

        public List<WorkerEquipmentInfo> WorkerEquipmentList { get; set; } = new List<WorkerEquipmentInfo>();
        public List<WorkerInfo> WorkersByDay { get; set; } = new List<WorkerInfo>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Запит для отримання інформації про робітників і обладнання
            var equipmentUsageCollection = _mongoDB.GetCollection<BsonDocument>("EquipmentUsage");
            var workersCollection = _mongoDB.GetCollection<BsonDocument>("Employees");

            // Отримуємо всі записи використання обладнання та робітників
            var usageRecords = await equipmentUsageCollection.Find(new BsonDocument()).ToListAsync();
            var workers = await workersCollection.Find(new BsonDocument()).ToListAsync();

            // Формуємо список робітників та обладнання, що вони видавали
            foreach (var record in usageRecords)
            {
                var employeeId = record["employee_id"].AsObjectId;
                var worker = workers.FirstOrDefault(w => w["_id"].AsObjectId == employeeId);

                if (worker != null)
                {
                    WorkerEquipmentList.Add(new WorkerEquipmentInfo
                    {
                        WorkerName = worker["name"].AsString,
                        Position = worker["position"].AsString,
                        EquipmentId = record["equipment_id"].AsObjectId,
                        Date = record["date"].ToLocalTime() // Коректна обробка дати
                    });
                }
            }

            // Запит для отримання робітників, які працювали в зазначену дату
            if (SelectedDate.HasValue)
            {
                var selectedDateOnly = SelectedDate.Value.Date;

                // Фільтруємо отримані записи за датою
                var filteredRecords = usageRecords
                    .Where(record => record["date"].ToUniversalTime().Date == selectedDateOnly)
                    .ToList();

                foreach (var record in filteredRecords)
                {
                    var employeeId = record["employee_id"].AsObjectId;
                    var worker = workers.FirstOrDefault(w => w["_id"].AsObjectId == employeeId);

                    if (worker != null)
                    {
                        WorkersByDay.Add(new WorkerInfo
                        {
                            WorkerName = worker["name"].AsString,
                            Position = worker["position"].AsString
                        });
                    }
                }
            }

            return Page();
        }
    }

    public class WorkerEquipmentInfo
    {
        public string WorkerName { get; set; }
        public string Position { get; set; }
        public ObjectId EquipmentId { get; set; }
        public DateTime Date { get; set; }
    }

    public class WorkerInfo
    {
        public string WorkerName { get; set; }
        public string Position { get; set; }
    }
}
