using ChessDotNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace AtomicChessPuzzles
{
    public static class Utilities
    {
        const int HASH_ITERATIONS = 100000;
        static readonly Regex allowedUserNameRegex = new Regex("^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);
        static readonly Regex emailRegex = new Regex(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", RegexOptions.Compiled);

        public static Tuple<string, string> HashPassword(string password)
        {
            Rfc2898DeriveBytes k = new Rfc2898DeriveBytes(password, 128, HASH_ITERATIONS);
            string key = Convert.ToBase64String(k.GetBytes(20));
            string salt = Convert.ToBase64String(k.Salt);
            return new Tuple<string, string>(key, salt);
        }

        public static string HashPassword(string password, string salt)
        {
            Rfc2898DeriveBytes k = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), HASH_ITERATIONS);
            string key = Convert.ToBase64String(k.GetBytes(20));
            return key;
        }

        public static string SanitizeHtml(string unsafeHtml)
        {
            return unsafeHtml.Replace("&", "&amp;")
                             .Replace("<", "&lt;")
                             .Replace(">", "&gt;")
                             .Replace("\"", "&quot;");
        }

        public static bool IsValidUsername(string username)
        {
            return allowedUserNameRegex.IsMatch(username);
        }

        public static bool IsValidEmail(string email)
        {
            return emailRegex.IsMatch(email);
        }

        public static Dictionary<string, List<string>> GetChessgroundDestsForMoveCollection(ReadOnlyCollection<Move> moves)
        {
            Dictionary<string, List<string>> dests = new Dictionary<string, List<string>>();
            foreach (Move m in moves)
            {
                string origin = m.OriginalPosition.ToString().ToLowerInvariant();
                string destination = m.NewPosition.ToString().ToLowerInvariant();
                if (!dests.ContainsKey(origin))
                {
                    dests.Add(origin, new List<string>());
                }
                dests[origin].Add(destination);
            }
            return dests;
        }
    }
}
