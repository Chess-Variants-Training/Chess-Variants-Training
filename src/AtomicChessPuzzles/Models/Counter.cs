using MongoDB.Bson.Serialization.Attributes;

namespace AtomicChessPuzzles.Models
{
    public class Counter
    {
        [BsonElement("_id")]
        public string ID { get; set; }

        [BsonElement("next")]
        public int Next { get; set; }

        public Counter(string id, int next)
        {
            ID = id;
            Next = next;
        }
    }
}
