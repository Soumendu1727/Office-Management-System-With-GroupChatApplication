using System.Security.Cryptography;
using System.Text;

namespace ClientServerCommunication.Services
{
    public static class MessageEncryptionHelper
    {
        // MUST be 16 / 24 / 32 bytes
        private static readonly byte[] Key =
            Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // 32 bytes = AES-256

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Store IV + CipherText
            byte[] combined = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(combined);
        }

        public static string Decrypt(string encryptedText)
        {
            byte[] combined = Convert.FromBase64String(encryptedText);

            using var aes = Aes.Create();
            aes.Key = Key;

            byte[] iv = new byte[aes.BlockSize / 8];
            byte[] cipherBytes = new byte[combined.Length - iv.Length];

            Buffer.BlockCopy(combined, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(combined, iv.Length, cipherBytes, 0, cipherBytes.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
