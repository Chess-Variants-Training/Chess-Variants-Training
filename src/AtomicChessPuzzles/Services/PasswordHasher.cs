using System;
using System.Security.Cryptography;

namespace AtomicChessPuzzles.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        const int HASH_ITERATIONS = 100000;

        public Tuple<string, string> HashPassword(string password)
        {
            Rfc2898DeriveBytes k = new Rfc2898DeriveBytes(password, 128, HASH_ITERATIONS);
            string key = Convert.ToBase64String(k.GetBytes(20));
            string salt = Convert.ToBase64String(k.Salt);
            return new Tuple<string, string>(key, salt);
        }

        public string HashPassword(string password, string salt)
        {
            Rfc2898DeriveBytes k = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), HASH_ITERATIONS);
            string key = Convert.ToBase64String(k.GetBytes(20));
            return key;
        }
    }
}