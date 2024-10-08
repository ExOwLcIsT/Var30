using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Var30.Models;
using System.Reflection.PortableExecutable;
using System;

public class IndexModel : PageModel
{
    private readonly IMongoDatabase _mongoDB;

    public IndexModel(IMongoDatabase mongoDB)
    {
        _mongoDB = mongoDB;
    }

    [BindProperty]
    public CollectionViewModel CollectionData { get; set; }

    static Random random = new Random();
    public async Task OnGetAsync()
    {
        var collections = await _mongoDB.ListCollectionNamesAsync();
        var collectionsList = await collections.ToListAsync();
        collectionsList.Remove("Keys");
        CollectionData = new CollectionViewModel
        {
            Login = (HttpContext.Request.Cookies["Username"] == null ? "" : HttpContext.Request.Cookies["Username"]),
            Collections = collectionsList,
            SelectedCollection = "",
            Documents = new List<Dictionary<string, object>>(),
            Headers = new List<string>()
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!string.IsNullOrEmpty(CollectionData.SelectedCollection))
        {
            var collections = await _mongoDB.ListCollectionNamesAsync();
            CollectionData.Collections = await collections.ToListAsync();
            CollectionData.Collections.Remove("Keys");
            CollectionData.Documents = new List<Dictionary<string, object>>();
            var collection = _mongoDB.GetCollection<BsonDocument>(CollectionData.SelectedCollection);
            var documents = await collection.Find(new BsonDocument()).ToListAsync();
            foreach (var doc in documents)
            {
                var dict = new Dictionary<string, object>();
                foreach (var element in doc.Elements)
                {
                    dict[element.Name] = element.Value;
                }
                CollectionData.Documents.Add(dict);
            }
            if (CollectionData.Documents.Count > 0)
            {
                CollectionData.Headers = new List<string>(CollectionData.Documents[0].Keys);
            }
            else
            {
                CollectionData.Headers = new List<string>();
            }
        }
        return Page();
    }
}
