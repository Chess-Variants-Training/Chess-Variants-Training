using MongoDB.Bson.Serialization.Attributes;

namespace ChessVariantsTraining.Models
{
    public class Counter
    {
        [BsonElement("_id")]
        public string ID { get; set; }

        [BsonElement("next")]
        public int Next { get; set; }

        public Counter() { }

        public Counter(string id, int next)
        {
            ID = id;
            Next = next;
        }

        public const string USER_ID = "userId";
        public const string PUZZLE_ID = "puzzleId";
        public const string COMMENT_ID = "commentId";
    }
}
