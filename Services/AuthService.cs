using System.Text.Json;
using ClientServerCommunication.Models;

namespace ClientServerCommunication.Services
{
    public class AuthService
    {
        private readonly string _userFilePath;

        public AuthService(IWebHostEnvironment env)
        {
            _userFilePath = Path.Combine(env.ContentRootPath, "Data", "users.json");
        }

        public User? Authenticate(string email, string password)
        {
            if (!File.Exists(_userFilePath))
                return null;

            var users = JsonSerializer.Deserialize<List<User>>(
                File.ReadAllText(_userFilePath)) ?? new();

            var user = users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user == null || !user.IsActive)
                return null;

            // 🔐 HASH COMPARISON
            return PasswordHelper.VerifyPassword(password, user.Password)
                ? user
                : null;
        }
    }
}
