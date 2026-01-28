using Microsoft.AspNetCore.Mvc.RazorPages;
using ClientServerCommunication.Services;
using ClientServerCommunication.Models;

namespace New_Project.Pages.Discussion
{
    public class IndexModel : PageModel
    {
        private readonly GroupService _groupService;

        public List<Group> Groups { get; set; } = new();

        public IndexModel(GroupService groupService)
        {
            _groupService = groupService;
        }

        public void OnGet()
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId")!);
            string role = HttpContext.Session.GetString("UserRole")!;

            var allGroups = _groupService.GetAllGroups();

            Groups = role switch
            {
                "Admin" => allGroups,
                "TeamLeader" => allGroups.Where(g => g.TeamLeaderIds.Contains(userId)).ToList(),
                _ => allGroups.Where(g => g.MemberIds.Contains(userId)).ToList()
            };
        }
    }
}
