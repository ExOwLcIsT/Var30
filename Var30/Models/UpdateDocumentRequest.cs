namespace Var30.Models
{
    public class UpdateDocumentRequest
    {
        public string Id { get; set; } // ID документа в MongoDB
        public string Field { get; set; } // Поле, яке змінюється
        public string Value { get; set; } // Нове значення поля
        public string DataType { get; set; } // Нове значення поля
        public string CollectionName { get; set; }
    }
}