using System.Text.Json;
using ClientServerCommunication.Models;


namespace ClientServerCommunication.Services
{
    public class GroupService
    {
        private readonly string _filePath;


        public GroupService(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "Data", "groups.json");
            if (!File.Exists(_filePath)) File.WriteAllText(_filePath, "[]");
        }


        public List<Group> GetAllGroups()
        {
            return JsonSerializer.Deserialize<List<Group>>(File.ReadAllText(_filePath)) ?? new();
        }


        public void CreateGroup(Group group)
        {
            var groups = GetAllGroups();
            group.Id = groups.Any() ? groups.Max(g => g.Id) + 1 : 1;
            groups.Add(group);
            Save(groups);
        }

        public Group? GetById(int groupId)
        {
            return GetAllGroups().FirstOrDefault(g => g.Id == groupId);
        }
        public void UpdateGroup(Group updatedGroup)
        {
            var groups = GetAllGroups();
            var group = groups.FirstOrDefault(g => g.Id == updatedGroup.Id);

            if (group == null) return;

            group.GroupName = updatedGroup.GroupName;
            group.TeamLeaderIds = updatedGroup.TeamLeaderIds;
            group.MemberIds = updatedGroup.MemberIds;

            Save(groups);
        }
        public void RemoveMember(int groupId, int userId)
        {
            var groups = GetAllGroups();
            var group = groups.FirstOrDefault(g => g.Id == groupId);
            if (group == null) return;


            group.MemberIds.Remove(userId);
            group.TeamLeaderIds.Remove(userId);
            Save(groups);
        }

        public void DeleteGroup(int groupId)
        {
            var groups = GetAllGroups();
            var group = groups.FirstOrDefault(g => g.Id == groupId);

            if (group == null)
                return;

            groups.Remove(group);
            Save(groups);
        }       


        private void Save(List<Group> groups)
        {
            File.WriteAllText(_filePath, JsonSerializer.Serialize(groups, new JsonSerializerOptions { WriteIndented = true }));
        }

        
    }
}