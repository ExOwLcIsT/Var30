namespace Var30.Models
{
    public class AddDocumentRequest
    {
        public string CollectionName { get; set; }
        public Dictionary<string, object?> Fields { get; set; } // Поля документа
    }

}
