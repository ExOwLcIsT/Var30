using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Var30.Services
{
    public class UserService
    {
        private readonly IMongoCollection<BsonDocument> _userKeys;
        private readonly List<string> roles = (new string[] { "guest", "operator", "admin", "owner" }).ToList();

        public UserService(IMongoDatabase database)
        {
            // Працюємо напряму з колекцією документів MongoDB
            _userKeys = database.GetCollection<BsonDocument>("Keys");
        }

        // Отримуємо користувача за ім'ям (пошук в колекції без конкретної моделі)
        public async Task<BsonDocument> GetUserByUsernameAsync(string username)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Username", username);
            return await _userKeys.Find(filter).FirstOrDefaultAsync();
        }

        // Додаємо нового користувача в колекцію документів
        public async Task<bool> AddUserAsync(BsonDocument userKey)
        {
            await _userKeys.InsertOneAsync(userKey);
            return true;
        }

        // Аутентифікація користувача за ім'ям та паролем
        public async Task<BsonDocument> AuthenticateUserAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);

            if (user != null && VerifyPassword(password, user["Password"].AsString))
            {
                return user;
            }
            return null;
        }

        // Перевіряємо пароль
        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            // Тут можна реалізувати більш складні перевірки паролів (наприклад, хешування)
            return enteredPassword == storedPassword;
        }
        // Перевірка, чи має користувач відповідну роль для доступу
        public bool UserHasAccess(string username, string requiredRole)
        {
            var user = GetUserByUsernameAsync(username).Result;
            return user != null && roles.IndexOf(user["Role"].AsString) >= roles.IndexOf(requiredRole);
        }
    }
}
