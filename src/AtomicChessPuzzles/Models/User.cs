using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace AtomicChessPuzzles.Models
{
    public class User
    {
        [BsonElement("_id")]
        public string Username { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("passwordhash")]
        public string PasswordHash { get; set; }

        [BsonElement("salt")]
        public string Salt { get; set; }

        public User(string username, string email, string passwordHash, string salt)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Salt = salt;
        }
    }
}
