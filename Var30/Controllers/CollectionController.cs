
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Var30.Services;
namespace Var30.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CollectionController : ControllerBase
    {
        private readonly IMongoDatabase _mongoDB;
        private readonly UserService _userService;

        public CollectionController(IMongoDatabase mongoDB, UserService userService)
        {
            _mongoDB = mongoDB;
            _userService = userService;
        }

        [HttpPost("add")]
        public IActionResult AddCollection([FromBody] string collectionName)
        {
            var user = this.HttpContext.Request.Cookies["Username"];
            if (user != null && _userService.UserHasAccess(user, "owner"))
            {
                _mongoDB.CreateCollection(collectionName);
                var emptyDocument = new BsonDocument{ { "Поле-пустишка", "віффвівфі" } };
                var collection = _mongoDB.GetCollection<BsonDocument>(collectionName);
                collection.InsertOne(emptyDocument);
                return Ok(new { success = true, message = "Колекцію успішно додано." });
            }
            return BadRequest(new { success = false, status = 403, message = "Недостатньо прав доступу." });
        }

        [HttpDelete("delete/{collectionName}")]
        public IActionResult DeleteCollection(string collectionName)
        {
            var user = this.HttpContext.Request.Cookies["Username"];
            if (user != null && _userService.UserHasAccess(user, "owner"))
            {
                _mongoDB.DropCollection(collectionName);
                return Ok(new { success = true, message = "Колекцію успішно видалено." });
            }
            return BadRequest(new { success = false, status = 403, message = "Недостатньо прав доступу." });
        }

        [HttpPut("rename")]
        public IActionResult RenameCollection([FromBody] RenameCollectionRequest request)
        {
            var user = this.HttpContext.Request.Cookies["Username"];
            if (user != null && _userService.UserHasAccess(user, "owner"))
            {
                var command = new BsonDocument
            {
                { "renameCollection", $"{request.OldName}" },
                { "to", $"{request.NewName}" }
            };
                _mongoDB.RunCommand<BsonDocument>(command);
                return Ok(new { success = true, message = "Колекцію успішно перейменовано." });
            }
            return BadRequest(new { success = false, status = 403, message = "Недостатньо прав доступу." });
        }
    }

    public class RenameCollectionRequest
    {
        public string OldName { get; set; }
        public string NewName { get; set; }
    }

}
