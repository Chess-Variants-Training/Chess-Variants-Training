using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;

namespace AtomicChessPuzzles.Models
{
    public class User
    {
        [BsonElement("_id")]
        public string ID { get; set; }

        [BsonElement("username")]
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

        [BsonElement("role")]
        public UserRole Role { get; set; }

        [BsonElement("rating")]
        public Rating Rating { get; set; }

        [BsonElement("solvedPuzzles")]
        public List<string> SolvedPuzzles { get; set; }

        public User(string id, string username, string email, string passwordHash, string salt, string about,
            int puzzlesCorrect, int puzzlesWrong, UserRole role, Rating rating, List<string> solvedPuzzles)
        {
            ID = id;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Salt = salt;
            About = about;
            PuzzlesCorrect = puzzlesCorrect;
            PuzzlesWrong = puzzlesWrong;
            Role = role;
            Rating = rating;
            SolvedPuzzles = solvedPuzzles;
        }
    }
}
