﻿using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using Var30.Services;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
public class FieldController : ControllerBase
{
    private readonly IMongoDatabase _mongoDB;

    private readonly UserService _userService;

    public FieldController(IMongoDatabase mongoDB, UserService userService)
    {
        _mongoDB = mongoDB;
        _userService = userService;
    }

    // Додавання нового поля до всіх документів
    [HttpPost("add-field")]
    public async Task<IActionResult> AddField([FromBody] AddFieldRequest request)
    {
        var user = this.HttpContext.Request.Cookies["Username"];
        if (user != null && _userService.UserHasAccess(user, "admin"))
        {
            var collection = _mongoDB.GetCollection<BsonDocument>(request.CollectionName);
            UpdateDefinition<BsonDocument> update;
            if (request.FieldType == "Number")
            {
                update = Builders<BsonDocument>.Update.Set(request.NewFieldName, BsonValue.Create(123.0));
            }
            else if (request.FieldType == "Date")
            {
                update = Builders<BsonDocument>.Update.Set(request.NewFieldName, BsonValue.Create(DateTime.Now));
            }
            else if (request.FieldType == "Boolean")
            {
                update = Builders<BsonDocument>.Update.Set(request.NewFieldName, BsonValue.Create(true));
            }
            else
            {

                update = Builders<BsonDocument>.Update.Set(request.NewFieldName, BsonValue.Create("new field"));
            }
            var result = await collection.UpdateManyAsync(Builders<BsonDocument>.Filter.Exists(request.NewFieldName, false), update);

            return Ok(new
            {
                success = true,
                modifiedCount = result.ModifiedCount
            });
        }
        else
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
    }

    // Видалення поля з усіх документів
    [HttpPost("remove-field")]
    public async Task<IActionResult> RemoveField([FromBody] RemoveFieldRequest request)
    {
        var user = this.HttpContext.Request.Cookies["Username"];
        if (user != null && _userService.UserHasAccess(user, "admin"))
        {
            var collection = _mongoDB.GetCollection<BsonDocument>(request.CollectionName);

            // Видаляємо поле з усіх документів
            var update = Builders<BsonDocument>.Update.Unset(request.FieldName);
            var result = await collection.UpdateManyAsync(Builders<BsonDocument>.Filter.Exists(request.FieldName), update);

            return Ok(new { success = true, modifiedCount = result.ModifiedCount });
        }
        else
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
    }

    // Перейменування поля в усіх документах
    [HttpPost("rename-field")]
    public async Task<IActionResult> RenameField([FromBody] RenameFieldRequest request)
    {
        var user = this.HttpContext.Request.Cookies["Username"];
        if (user != null && _userService.UserHasAccess(user, "admin"))
        {
            var collection = _mongoDB.GetCollection<BsonDocument>(request.CollectionName);
            var filter = Builders<BsonDocument>.Filter.Ne(request.OldFieldName, BsonNull.Value); // Фільтр для документів, де поле "nmae" не є null
            var update = Builders<BsonDocument>.Update.Rename(request.OldFieldName, request.NewFieldName); // Операція перейменування поля

            var result = await collection.UpdateManyAsync(filter, update);

            return Ok(new { success = true, status = 200 });
        }
        else
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
    }
}

public class AddFieldRequest
{
    public string CollectionName { get; set; }
    public string NewFieldName { get; set; }
    public string FieldType { get; set; }
}

public class RemoveFieldRequest
{
    public string CollectionName { get; set; }
    public string FieldName { get; set; }
}

public class RenameFieldRequest
{
    public string CollectionName { get; set; }
    public string OldFieldName { get; set; }
    public string NewFieldName { get; set; }
}
