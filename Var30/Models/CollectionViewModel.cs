using MongoDB.Bson;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;

namespace Var30.Models
{
    public class CollectionViewModel
    {
        public string Login { get; set; }
        public List<string> Collections { get; set; }
        public string SelectedCollection { get; set; }
        public List<Dictionary<string, object>> Documents { get; set; }
        public List<string> Headers { get; set; }
    }
}
