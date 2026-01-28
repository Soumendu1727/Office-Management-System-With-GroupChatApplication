using System.Text.Json;
using ClientServerCommunication.Models;

namespace ClientServerCommunication.Services
{
    public class MessageService
    {
        private readonly string _filePath;

        public MessageService(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "Data", "messages.json");

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        // 🔹 Get all messages for a group discussion
        public List<Message> GetMessagesForGroup(int groupId)
        {
            return Read()
                .Where(m => m.GroupId == groupId)
                .OrderBy(m => m.SentAt)
                .ToList();
        }

        // 🔹 Get private messages between two users
        public List<Message> GetPrivateMessages(int user1Id, int user2Id)
        {
            return Read()
                .Where(m =>
                    m.GroupId == null &&
                    (
                        (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                        (m.SenderId == user2Id && m.ReceiverId == user1Id)
                    )
                )
                .OrderBy(m => m.SentAt)
                .ToList();
        }

        // 🔹 Add a new message (group or private)
        public void AddMessage(Message message)
        {
            var messages = Read();

            message.Id = messages.Any()
                ? messages.Max(m => m.Id) + 1
                : 1;

            message.SentAt = DateTime.Now;

            messages.Add(message);
            Save(messages);
        }

        // 🔹 Read all messages from JSON
        private List<Message> Read()
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Message>>(json) ?? new List<Message>();
        }

        // 🔹 Save messages to JSON
        private void Save(List<Message> messages)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            File.WriteAllText(_filePath, JsonSerializer.Serialize(messages, options));
        }
    }
}
