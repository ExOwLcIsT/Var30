using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using Var30.Models;
using Var30.Services;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly IMongoDatabase _mongoDB;
    private readonly UserService _userService;
    public DocumentController(IMongoDatabase mongoDB, UserService userService)
    {
        _mongoDB = mongoDB;
        _userService = userService;
    }

    [HttpPost("update-document")]
    public async Task<IActionResult> UpdateDocument([FromBody] UpdateDocumentRequest request)
    {
        var user = this.HttpContext.Request.Cookies["Username"];
        if (user != null && _userService.UserHasAccess(user, "operator"))
        {
            var collection = _mongoDB.GetCollection<BsonDocument>(request.CollectionName);

            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(request.Id));

            UpdateDefinition<BsonDocument> update = Builders<BsonDocument>.Update.Set(request.Field, BsonValue.Create(request.Value));

            if (request.DataType == "Int32" || request.DataType == "Double" || request.DataType == "Float" || request.DataType == "Decimal")
            {
                Builders<BsonDocument>.Update.Set(request.Field, BsonValue.Create(decimal.Parse(request.Value)));
            }

            else if (request.DataType == "BsonBoolean")
            {
                Builders<BsonDocument>.Update.Set(request.Field, BsonValue.Create(bool.Parse(request.Value)));
            }
            var result = await collection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                return Ok(new { success = true, status = 200 });
            }

            return BadRequest(new { success = false, error = "Не вдалося оновити документ", status = 400 });
        }
        else
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
    }
    [HttpPost("add-document")]
    public async Task<IActionResult> AddDocument([FromBody] AddDocumentRequest request)
    {
        var role = this.HttpContext.Request.Cookies["Role"];
        if (role != null && _userService.UserHasAccess(role, "operator"))
        {
            var collection = _mongoDB.GetCollection<BsonDocument>(request.CollectionName);

            string jsonString = JsonSerializer.Serialize(request.Fields);

            var bsonDocument = BsonDocument.Parse(jsonString);
            if (bsonDocument["_id"] != null)
            {
                bsonDocument.Remove("_id");
            }
            await collection.InsertOneAsync(bsonDocument);

            return Ok(new { success = true, status = 201, documentId = bsonDocument["_id"].ToString() });
        }
        else
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
    }
    [HttpPost("delete-document")]
    public async Task<IActionResult> DeleteDocument([FromBody] DeleteDocumentRequest request)
    {
        var role = this.HttpContext.Request.Cookies["Role"];
        if (role != null && _userService.UserHasAccess(role, "operator"))
        {
            var collection = _mongoDB.GetCollection<BsonDocument>(request.CollectionName);

            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(request.Id));

            var result = await collection.DeleteOneAsync(filter);

            if (result.DeletedCount > 0)
            {
                return Ok(new { success = true, status = 200 });
            }

            return BadRequest(new { success = false, error = "Не вдалося видалити документ", status = 400 });
        }
        else
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
    }

}
