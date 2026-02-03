using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClientServerCommunication.Services;
using ClientServerCommunication.Models;
using Microsoft.AspNetCore.SignalR;

namespace New_Project.Pages.Discussion
{
    public class GroupModel : PageModel
    {
        private readonly GroupService _groupService;
        private readonly MessageService _messageService;
        private readonly UserService _userService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly FileService _fileService;

        public Group Group { get; set; } = new();
        public List<Group> Groups { get; set; } = new();
        public Dictionary<int, string> UserNames { get; set; } = new();

        // 🔥 Unified timeline
        public List<ChatItem> Timeline { get; set; } = new();

        [BindProperty]
        public string Contents { get; set; } = string.Empty;

        public GroupModel(
            GroupService groupService,
            MessageService messageService,
            UserService userService,
            IHubContext<ChatHub> hubContext,
            FileService fileService)
        {
            _groupService = groupService;
            _messageService = messageService;
            _userService = userService;
            _hubContext = hubContext;
            _fileService = fileService;
        }

        // ================= GET =================
        public IActionResult OnGet(int groupId)
        {
            if (!TryGetUserId(out int userId, out string role))
                return RedirectToPage("/AccessDenied");

            if (groupId <= 0)
                return RedirectToPage("/AccessDenied");

            Group = _groupService.GetById(groupId);

            if (Group == null || !IsAllowed(Group, userId))
                return RedirectToPage("/AccessDenied");

            var allGroups = _groupService.GetAllGroups();
            Groups = role switch
            {
                "Admin" => allGroups,
                "TeamLeader" => allGroups.Where(g => g.TeamLeaderIds.Contains(userId)).ToList(),
                _ => allGroups.Where(g => g.MemberIds.Contains(userId)).ToList()
            };

            // Users
            UserNames = _userService.GetAllUsers()
                .ToDictionary(u => u.Id, u => u.Name);

            // Messages → timeline
            var messages = _messageService.GetMessagesForGroup(groupId)
                .Select(m => new ChatItem
                {
                    Id = m.Id,
                    GroupId = m.GroupId,
                    SenderId = m.SenderId,
                    MessageText = MessageEncryptionHelper.Decrypt(m.Contents),
                    SentAt = m.SentAt
                });

            // Files → timeline
            var files = _fileService.GetFilesByGroup(groupId)
                .Select(f => new ChatItem
                {
                    Id = f.Id,
                    GroupId = f.GroupId,
                    SenderId = f.UploadedBy,
                    FileName = f.FileName,
                    FilePath = f.FilePath,
                    SentAt = f.UploadedAt
                });

            // 🔥 Merge + sort
            Timeline = messages
                .Concat(files)
                .OrderBy(t => t.SentAt)
                .ToList();

            foreach (var item in Timeline)
            {
                if (!UserNames.ContainsKey(item.SenderId))
                {
                    UserNames[item.SenderId] = $"User-{item.SenderId}";
                }
            }

            return Page();
        }

        // ================= POST =================
        public async Task<IActionResult> OnPostAsync(int groupId, IFormFile[]? uploadedFiles)
        {
            
            if (!TryGetUserId(out int userId, out _))
                return RedirectToPage("/AccessDenied");

            Group = _groupService.GetById(groupId);
            if (Group == null || !IsAllowed(Group, userId))
                return RedirectToPage("/AccessDenied");

            var userName = _userService.GetAllUsers()
                .FirstOrDefault(u => u.Id == userId)?.Name ?? $"User-{userId}";

            // Text
            if (!string.IsNullOrWhiteSpace(Contents))
            {
                _messageService.AddMessage(new Message
                {
                    GroupId = groupId,
                    SenderId = userId,
                    Contents = MessageEncryptionHelper.Encrypt(Contents)
                });

                await _hubContext.Clients.Group(groupId.ToString())
                    .SendAsync("ReceiveMessage", userName, Contents, userId);
            }

            // File
            if (uploadedFiles != null && uploadedFiles.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/group-files");
                Directory.CreateDirectory(folder);

                foreach (var file in uploadedFiles)
                {
                    if (file.Length == 0) continue;

                    var uniqueName = $"{Guid.NewGuid()}_{file.FileName}";
                    var path = Path.Combine(folder, uniqueName);

                    await using var stream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(stream);

                    var savedFile = _fileService.AddFile(new SharedFile
                    {
                        GroupId = groupId,
                        FileName = file.FileName,
                        FilePath = $"/uploads/group-files/{uniqueName}",
                        UploadedBy = userId
                    });

                    await _hubContext.Clients.Group(groupId.ToString())
                        .SendAsync("ReceiveFile",
                            userName,
                            savedFile.FileName,
                            savedFile.Id,
                            userId,
                            savedFile.UploadedAt.ToString("dd/MM/yy , hh:mm tt"));
                }

                
            }

            return RedirectToPage(new { groupId });
        }

        // ================= DOWNLOAD =================
        public IActionResult OnGetDownloadFile(int fileId)
        {
            var file = _fileService.GetFileById(fileId);
            if (file == null) return NotFound();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.TrimStart('/'));
            return PhysicalFile(path, "application/octet-stream", file.FileName);
        }

        private bool IsAllowed(Group g, int userId)
        {
            return g.CreatedByAdminId == userId
                || g.TeamLeaderIds.Contains(userId)
                || g.MemberIds.Contains(userId);
        }

        private bool TryGetUserId(out int userId, out string role)
        {
            userId = 0;
            role = string.Empty;

            var uid = HttpContext.Session.GetString("UserId");
            var r = HttpContext.Session.GetString("UserRole");

            if (uid == null || r == null) return false;

            userId = int.Parse(uid);
            role = r;
            return true;
        }
    }
}
