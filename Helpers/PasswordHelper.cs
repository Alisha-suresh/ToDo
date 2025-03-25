using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public static class PasswordHelper
{
    // PBKDF2 hashing
    public static string HashPassword(string password)
    {
        using (var hmac = new HMACSHA256())
        {
            // Generate a random salt
            var salt = GenerateSalt();
            hmac.Key = salt;

            
            var hashedPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hashedPassword)}";
        }
    }

    // Verify if password matches stored hash
    public static bool VerifyPassword(string password, string storedHash)
    {
        
        var parts = storedHash.Split(':');
        var salt = Convert.FromBase64String(parts[0]);
        var storedHashedPassword = Convert.FromBase64String(parts[1]);

        using (var hmac = new HMACSHA256())
        {
            hmac.Key = salt;
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            
            return computedHash.SequenceEqual(storedHashedPassword);
        }
    }

    // Random salt generation
    private static byte[] GenerateSalt()
    {
        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }
}