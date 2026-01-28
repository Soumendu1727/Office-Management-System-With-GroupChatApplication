namespace ClientServerCommunication.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        // Admin | TeamLeader | User
        public string Role { get; set; } = string.Empty;

        // Only for Users (null for Admin & TeamLeader)
        public int? TeamLeaderId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}