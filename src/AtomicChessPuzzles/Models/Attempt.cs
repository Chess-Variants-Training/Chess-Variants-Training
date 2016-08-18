using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AtomicChessPuzzles.Models
{
    public class Attempt
    {
        public ObjectId Id { get; set; }

        [BsonElement("user")]
        public int User { get; set; }

        [BsonElement("puzzleId")]
        public string PuzzleId { get; set; }

        [BsonElement("startTimestampUtc")]
        public DateTime StartTimestampUtc { get; set; }

        [BsonElement("endTimestampUtc")]
        public DateTime EndTimestampUtc { get; set; }

        [BsonElement("userRatingChange")]
        public double UserRatingChange { get; set; }

        [BsonElement("puzzleRatingChange")]
        public double PuzzleRatingChange { get; set; }

        [BsonElement("seconds")]
        public double Seconds { get; set; }

        [BsonElement("success")]
        public bool Success { get; set; }

        public Attempt(int user, string puzzleId, DateTime startTimestampUtc, DateTime endTimestampUtc, double userRatingChange, double puzzleRatingChange, bool success)
        {
            User = user;
            PuzzleId = puzzleId;
            StartTimestampUtc = startTimestampUtc;
            EndTimestampUtc = endTimestampUtc;
            UserRatingChange = userRatingChange;
            PuzzleRatingChange = puzzleRatingChange;
            Success = success;
        }
    }
}
