using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;

namespace ChessVariantsTraining.Models
{
    public class User
    {
        [BsonElement("_id")]
        public int ID { get; set; }

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

        [BsonElement("roles")]
        public List<string> Roles { get; set; }

        [BsonElement("ratings")]
        public Dictionary<string, Rating> Ratings { get; set; }

        [BsonElement("solvedPuzzles")]
        public List<int> SolvedPuzzles { get; set; }

        [BsonElement("verified")]
        public bool Verified { get; set; }

        [BsonElement("verificationCode")]
        public int VerificationCode { get; set; }

        public User() { }

        public User(int id, string username, string email, string passwordHash, string salt, string about,
            int puzzlesCorrect, int puzzlesWrong, List<string> roles, Dictionary<string, Rating> ratings, List<int> solvedPuzzles)
        {
            ID = id;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Salt = salt;
            About = about;
            PuzzlesCorrect = puzzlesCorrect;
            PuzzlesWrong = puzzlesWrong;
            Roles = roles;
            Ratings = ratings;
            SolvedPuzzles = solvedPuzzles;

            Verified = false;
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] buffer = new byte[8];
                rng.GetBytes(buffer);
                VerificationCode = Math.Abs(BitConverter.ToInt32(buffer, 0));
            }
        }
    }
}
