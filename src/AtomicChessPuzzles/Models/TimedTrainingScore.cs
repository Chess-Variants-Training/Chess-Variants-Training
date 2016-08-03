using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AtomicChessPuzzles.Models
{
    public class TimedTrainingScore
    {
        public ObjectId Id { get; set; }

        [BsonElement("score")]
        public double Score { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("owner")]
        public string Owner { get; set; }

        [BsonElement("dateRecorded")]
        public DateTime DateRecordedUtc { get; set; }

        public TimedTrainingScore(double score, string type, string owner, DateTime dateRecordedUtc)
        {
            Score = score;
            Type = type;
            Owner = owner;
            DateRecordedUtc = dateRecordedUtc;
        }
    }
}