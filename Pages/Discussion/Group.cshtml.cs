using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClientServerCommunication.Services;
using ClientServerCommunication.Models;

namespace New_Project.Pages.Discussion
{
    public class GroupModel : PageModel
    {
        private readonly GroupService _groupService;
        private readonly MessageService _messageService;
        private readonly UserService _userService;

        public Group Group { get; set; } = new();

        public List<Group> Groups { get; set; } = new();
        public List<Message> Messages { get; set; } = new();

        public Dictionary<int, string> UserNames { get; set; } = new();

        [BindProperty]
        public string Contents { get; set; } = string.Empty;

        public GroupModel(GroupService groupService, MessageService messageService, UserService userService)
        {
            _groupService = groupService;
            _messageService = messageService;
            _userService = userService;
        }

        public IActionResult OnGet(int groupId)
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId")!);
            string role = HttpContext.Session.GetString("UserRole")!;
            Group = _groupService.GetById(groupId)!;

            if (Group == null || !IsAllowed(Group, userId))
                return RedirectToPage("/AccessDenied");
            
            var allGroups = _groupService.GetAllGroups();
            Groups = role switch
            {
                "Admin" => allGroups,
                "TeamLeader" => allGroups.Where(g => g.TeamLeaderIds.Contains(userId)).ToList(),
                _ => allGroups.Where(g => g.MemberIds.Contains(userId)).ToList()
            };


            Messages = _messageService.GetMessagesForGroup(groupId);
            foreach (var msg in Messages)
            {
                msg.Contents = MessageEncryptionHelper.Decrypt(msg.Contents);
            }
            UserNames = _userService.GetAllUsers()
                .ToDictionary(u => u.Id, u => u.Name);
                
            foreach (var msg in Messages)
            {
                if (!UserNames.ContainsKey(msg.SenderId))
                {
                    UserNames[msg.SenderId] = $"User-{msg.SenderId}";
                }
            }
            return Page();
        }

        public IActionResult OnPost(int groupId)
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId")!);
            string role = HttpContext.Session.GetString("UserRole")!;

            Group = _groupService.GetById(groupId)!;

            if (Group == null || !IsAllowed(Group, userId))
                return RedirectToPage("/AccessDenied");

            _messageService.AddMessage(new Message
            {
                GroupId = groupId,
                SenderId = userId,
                Contents = MessageEncryptionHelper.Encrypt(Contents)
            });

            return RedirectToPage(new { groupId });
        }

        private bool IsAllowed(Group g, int userId)
        {
            return g.CreatedByAdminId == userId
                || g.TeamLeaderIds.Contains(userId)
                || g.MemberIds.Contains(userId);
        }
    }
}
