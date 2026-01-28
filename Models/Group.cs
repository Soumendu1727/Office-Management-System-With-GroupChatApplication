namespace ClientServerCommunication.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public List<int> TeamLeaderIds { get; set; } = new();
        public List<int> MemberIds { get; set; } = new();
        public int CreatedByAdminId { get; set; }
    }
}