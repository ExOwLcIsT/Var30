namespace Var30.Models
{
    public class DeleteDocumentRequest
    {
        public string CollectionName { get; set; }
        public string Id { get; set; } // Ідентифікатор документа для видалення
    }

}
