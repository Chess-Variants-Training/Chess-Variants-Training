using System;
using System.Security.Cryptography;

namespace AtomicChessPuzzles
{
    public static class PasswordUtilities
    {
        const int ITERATIONS = 100000;

        public static Tuple<string, string> HashPassword(string password)
        {
            Rfc2898DeriveBytes k = new Rfc2898DeriveBytes(password, 128, ITERATIONS);
            string key = Convert.ToBase64String(k.GetBytes(20));
            string salt = Convert.ToBase64String(k.Salt);
            return new Tuple<string, string>(key, salt);
        }

        public static string HashPassword(string password, string salt)
        {
            Rfc2898DeriveBytes k = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), ITERATIONS);
            string key = Convert.ToBase64String(k.GetBytes(20));
            return key;
        }
    }
}
