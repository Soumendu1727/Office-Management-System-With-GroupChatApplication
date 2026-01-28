using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using ClientServerCommunication.Models;

namespace ClientServerCommunication.Services
{
    public class UserService
    {
        private readonly string _filePath;

        public UserService(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "Data", "users.json");

            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        public List<User> GetAllUsers()
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new();
        }

        public void AddUser(User user, string plainPassword)
        {
            var users = GetAllUsers();

            user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
            user.Password = HashPassword(plainPassword);

            users.Add(user);

            File.WriteAllText(_filePath,
                JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true }));
        }

        public void UpdateUser(User updatedUser)
        {
            var users = GetAllUsers();
            var user = users.FirstOrDefault(u => u.Id == updatedUser.Id);

            if (user == null) return;

            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            user.Role = updatedUser.Role;
            user.TeamLeaderId = updatedUser.TeamLeaderId;

            Save(users);
        }

        public void DeleteUser(int userId)
        {
            var users = GetAllUsers();
            users.RemoveAll(u => u.Id == userId);
            Save(users);
        }

        private void Save(List<User> users)
        {
            File.WriteAllText(_filePath,
                JsonSerializer.Serialize(users, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
        }

        // 🔐 Password hashing
        private string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            byte[] hash = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                100_000,
                32);

            return $"{Convert.ToBase64String(salt)}|{Convert.ToBase64String(hash)}";
        }
    }
}
