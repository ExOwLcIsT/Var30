
namespace Var30.Models
{
    public class AddDocumentRequest
    {
        // Назва колекції, до якої буде додано документ
        public string CollectionName { get; set; }

        // Поля документа з їхніми значеннями
        public Dictionary<string, object?> Fields { get; set; }

        // Типи даних для кожного поля, що додається
        public Dictionary<string, string> DataTypes { get; set; }
    }

}
