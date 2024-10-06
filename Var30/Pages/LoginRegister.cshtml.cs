using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using Var30.Models;
using Var30.Services;

public class LoginRegisterModel : PageModel
{
    private readonly UserService _userService;

    public LoginRegisterModel(UserService userService)
    {
        _userService = userService;
    }

    [BindProperty]
    public string Username { get; set; }

    [BindProperty]
    public string Password { get; set; }

    [BindProperty]
    public string Role { get; set; }

    public string Message { get; set; }

    // Перевіряємо, чи залогінений користувач
    public bool IsUserLoggedIn => HttpContext.Request.Cookies.ContainsKey("Username");

    // Метод для логіну та реєстрації
    public async Task<IActionResult> OnPostAsync(string action)
    {
        if (action == "login")
        {
            var user = await _userService.AuthenticateUserAsync(Username, Password);
            if (user != null)
            {
                HttpContext.Response.Cookies.Append("Username", user["Username"].AsString);
                HttpContext.Response.Cookies.Append("Role", user["Role"].AsString);
                return RedirectToPage("/Index");
            }
            Message = "Невірне ім'я користувача або пароль.";
        }
        else if (action == "register")
        {
            var newUser = new MongoDB.Bson.BsonDocument
            {
                { "Username" , Username },
                { "Password" , Password },
                { "Role" , Role }
            };

            var result = await _userService.AddUserAsync(newUser);
            if (result)
            {
                HttpContext.Response.Cookies.Append("Username", newUser["Username"].AsString);
                HttpContext.Response.Cookies.Append("Role", newUser["Role"].AsString);
                RedirectToPage("/Index");
            }
            else
            {
                Message = "Реєстрація не вдалася.";
            }
        }
        return Page();
    }

    // Метод для логауту
    public async Task<IActionResult> OnPostLogout()
    {
        HttpContext.Response.Cookies.Delete("Username");
        HttpContext.Response.Cookies.Delete("Role");
        return RedirectToPage("/LoginRegister");
    }
}
