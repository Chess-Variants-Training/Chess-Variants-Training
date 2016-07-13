using MongoDB.Bson.Serialization.Attributes;

namespace AtomicChessPuzzles.Models
{
    public class TimedTrainingScore
    {
        [BsonElement("score")]
        public double Score { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("owner")]
        public string Owner { get; set; }

        public TimedTrainingScore(double score, string type, string owner)
        {
            Score = score;
            Type = type;
            Owner = owner;
        }
    }
}