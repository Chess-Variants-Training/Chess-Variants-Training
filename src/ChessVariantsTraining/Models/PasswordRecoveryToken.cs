using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Security.Cryptography;

namespace ChessVariantsTraining.Models
{
    public class PasswordRecoveryToken
    {
        const int HASH_ITERATIONS = 10000;

        [BsonElement("tokenHashed")]
        public string TokenHashed { get; set; }

        [BsonIgnore]
        public string TokenUnhashed { get; set; }

        [BsonElement("expiry")]
        public DateTime Expiry { get; set; }

        public PasswordRecoveryToken()
        {
            byte[] tokenBuffer = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBuffer);
            }
            TokenUnhashed = Convert.ToBase64String(tokenBuffer);
            TokenHashed = GetHashedFor(TokenUnhashed);
            Expiry = DateTime.UtcNow.AddDays(1);
        }

        public static string GetHashedFor(string token)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return Convert.ToBase64String(sha256.ComputeHash(Convert.FromBase64String(token)));
            }
        }
    }
}
