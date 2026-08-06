using System.Security.Cryptography;
using System.Text;

// Shared password hashing for the Admin login. SHA-256, lowercase hex —
// must stay in sync with how Admin_Schema.sql seeds/hashes passwords in SQL
// (HASHBYTES('SHA2_256', ...) converted with style 2, then LOWER()).
public static class AdminAuthHelper
{
    public static string HashPassword(string plainPassword)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainPassword));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
