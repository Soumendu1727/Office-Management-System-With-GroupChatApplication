using System.Text.Json;

namespace ClientServerCommunication.Services
{
    public static class JsonFileService
    {
        private static readonly object _lock = new();

        // READ JSON FILE
        public static List<T> Read<T>(string filePath)
        {
            lock (_lock)
            {
                if (!File.Exists(filePath))
                {
                    return new List<T>();
                }

                var json = File.ReadAllText(filePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<T>();
                }

                return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
            }
        }

        // WRITE JSON FILE
        public static void Write<T>(string filePath, List<T> data)
        {
            lock (_lock)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(data, options);
                File.WriteAllText(filePath, json);
            }
        }
    }
}
