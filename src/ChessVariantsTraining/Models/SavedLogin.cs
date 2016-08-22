using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Security.Cryptography;

namespace ChessVariantsTraining.Models
{
    public class SavedLogin
    {
        [BsonElement("_id")]
        public long ID { get; set; }

        [BsonElement("hashedToken")]
        public byte[] HashedToken { get; set; }

        [BsonElement("user")]
        public int User { get; set; }

        [BsonIgnore]
        public string UnhashedToken { get; set; }

        public SavedLogin(int user)
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] bytes = new byte[8 + 64];
                rng.GetBytes(bytes);
                ID = BitConverter.ToInt64(bytes, 0);

                UnhashedToken = BitConverter.ToString(bytes, 8).Replace("-", "");
                using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                {
                    HashedToken = sha256.ComputeHash(bytes, 8, 64);
                }
            }
            User = user;
        }
    }
}
