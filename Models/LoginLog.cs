using System;

namespace ClientServerCommunication.Models
{
    public class LoginLog
    {
        public int UserId { get; set; }

        public int? TeamLeaderId { get; set; }

        public DateTime LoginTime { get; set; }

        public DateTime? LogoutTime { get; set; }
    }
}
