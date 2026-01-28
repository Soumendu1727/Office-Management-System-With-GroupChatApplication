using System;

namespace ClientServerCommunication.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int SenderId { get; set; }

        public int ReceiverId { get; set; }

        public int? GroupId { get; set; }

        public string Contents { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }
}
