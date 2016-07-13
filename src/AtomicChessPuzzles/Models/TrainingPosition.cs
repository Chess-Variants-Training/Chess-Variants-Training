using MongoDB.Bson.Serialization.Attributes;

namespace AtomicChessPuzzles.Models
{
    public class TrainingPosition
    {
        [BsonElement("_id")]
        public string ID { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("fen")]
        public string FEN { get; set; }
    }
}