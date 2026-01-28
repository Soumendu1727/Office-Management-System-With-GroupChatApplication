using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClientServerCommunication.Services;

namespace ClientServerCommunication.Pages
{
    public class LoginModel : PageModel
    {
        private readonly AuthService _authService;

        public LoginModel(AuthService authService)
        {
            _authService = authService;
        }

        // 🧾 Form Inputs
        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        // ❌ Error Message
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // Just render login page
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Email and Password are required.";
                return Page();
            }

            var user = _authService.Authenticate(Email, Password);

            if (user == null || !user.IsActive)
            {
                ErrorMessage = "Invalid login credentials.";
                return Page();
            }

            // 🔐 Store session values
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);

            // 🔁 Role-based redirect
            // 🔁 ROLE BASED ENTRY POINT
            if (user.Role == "Admin")
            {
                return RedirectToPage("/Admin/Dashboard");
            }

            // TeamLeader & User go to discussion
            return RedirectToPage("/Discussion/Index");
        }
    }
}
