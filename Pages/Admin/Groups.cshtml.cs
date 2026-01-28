using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClientServerCommunication.Services;
using ClientServerCommunication.Models;
using Microsoft.AspNetCore.Mvc.Filters;


namespace New_Project.Pages.Admin
{
    public class GroupsModel : PageModel
    {
        private readonly GroupService _groupService;
        private readonly UserService _userService;


        public GroupsModel(GroupService groupService, UserService userService)
        {
            _groupService = groupService;
            _userService = userService;
        }


        public List<Group> Groups { get; set; } = new();
        public List<User> Users { get; set; } = new();


        [BindProperty]
        public string GroupName { get; set; } = string.Empty;


        [BindProperty]
        public List<int> SelectedTeamLeaders { get; set; } = new();


        [BindProperty]
        public List<int> SelectedMembers { get; set; } = new();

        [BindProperty]
        public Group EditGroup { get; set; } = new();

        public List<User> TeamLeaders { get; set; } = new();
        public List<User> Members { get; set; } = new();


        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToPage("/AccessDenied");



            Groups = _groupService.GetAllGroups();
            Users = _userService.GetAllUsers();

            TeamLeaders = Users.Where(u => u.Role == "TeamLeader").ToList();
            Members = Users.Where(u => u.Role == "User").ToList();
            return Page();
        }

        public IActionResult OnPostDeleteGroup(int groupId)
        {
            _groupService.DeleteGroup(groupId);
            return RedirectToPage();
        }


        public IActionResult OnPostCreate()
        {
            var adminId = int.Parse(HttpContext.Session.GetString("UserId")!);


            var group = new Group
            {
                GroupName = GroupName,
                TeamLeaderIds = SelectedTeamLeaders,
                MemberIds = SelectedMembers,
                CreatedByAdminId = adminId
            };


            _groupService.CreateGroup(group);
            return RedirectToPage();
        }

        public IActionResult OnPostUpdateGroup()
        {

            _groupService.UpdateGroup(EditGroup);
            return RedirectToPage();
        }


        public IActionResult OnPostRemoveMember(int groupId, int userId)
        {
            _groupService.RemoveMember(groupId, userId);
            return RedirectToPage();
        }
    }
}