using System.Security.Cryptography;
using System.Text;

namespace ProductApp.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
