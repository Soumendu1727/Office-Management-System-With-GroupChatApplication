using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace New_Project.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        public string AdminName { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            // 🔒 Check login
            var role = HttpContext.Session.GetString("UserRole");

            if (role == null)
            {
                return RedirectToPage("/Login");
            }

            // 🔐 Admin-only access
            if (role != "Admin")
            {
                return RedirectToPage("/AccessDenied");
            }

            AdminName = HttpContext.Session.GetString("UserName") ?? "Admin";

            return Page();
        }
    }
}
