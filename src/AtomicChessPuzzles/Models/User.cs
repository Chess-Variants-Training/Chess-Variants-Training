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

        [BsonElement("about")]
        public string About { get; set; }

        [BsonElement("puzzlescorrect")]
        public int PuzzlesCorrect { get; set; }

        [BsonElement("puzzleswrong")]
        public int PuzzlesWrong { get; set; }

        public User(string username, string email, string passwordHash, string salt, int puzzlesCorrect, int puzzlesWrong)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Salt = salt;
            PuzzlesCorrect = puzzlesCorrect;
            PuzzlesWrong = puzzlesWrong;
        }
    }
}
