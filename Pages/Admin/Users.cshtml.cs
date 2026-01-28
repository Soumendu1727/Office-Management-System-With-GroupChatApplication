using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClientServerCommunication.Models;
using ClientServerCommunication.Services;

namespace New_Project.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly UserService _userService;

        public UsersModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public User NewUser { get; set; } = new();

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public List<User> Users { get; set; } = new();

        [BindProperty]
        public int EditUserId { get; set; }

        [BindProperty]
        public User EditUser { get; set; } = new();

        public List<User> TeamLeaders { get; set; } = new();

        public IActionResult OnPostUpdate()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToPage("/AccessDenied");

            if (EditUser.Role != "User")
            {
                EditUser.TeamLeaderId = null;
            }

            _userService.UpdateUser(EditUser);
            return RedirectToPage();
        }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToPage("/AccessDenied");

            Users = _userService
                    .GetAllUsers()
                    .OrderBy(u =>
                        u.Role == "Admin" ? 1 :
                        u.Role == "TeamLeader" ? 2 : 3)
                    .ThenBy(u => u.Name)
                    .ToList();
            TeamLeaders = Users.Where(u => u.Role == "TeamLeader").ToList();

            return Page();
        }


        public IActionResult OnPostDelete(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToPage("/AccessDenied");

            _userService.DeleteUser(id);
            return RedirectToPage();
        }

        public IActionResult OnPost()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToPage("/AccessDenied");

            if (string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError("", "Password is required");
                Users = _userService.GetAllUsers();
                TeamLeaders = Users.Where(u => u.Role == "TeamLeader").ToList();
                return Page();
            }

            if (NewUser.Role != "User")
            {
                NewUser.TeamLeaderId = null;
            }

            _userService.AddUser(NewUser, Password);
            return RedirectToPage();
        }

    }
}
