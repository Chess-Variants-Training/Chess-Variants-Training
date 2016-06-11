using MongoDB.Bson.Serialization.Attributes;

namespace AtomicChessPuzzles.Models
{
    public class Comment
    {
        [BsonElement("_id")]
        public string ID { get; set; }

        [BsonElement("author")]
        public string Author { get; set; }

        [BsonElement("bodyUnsanitized")]
        public string BodyUnsanitized { get; set; }

        public string BodySanitized
        {
            get
            {
                return Utilities.SanitizeHtml(BodyUnsanitized);
            }
        }

        [BsonElement("parentId")]
        public string ParentID { get; set; }

        [BsonElement("puzzleId")]
        public string PuzzleID { get; set; }

        [BsonElement("deleted")]
        public bool Deleted { get; set; }

        public Comment(string id, string author, string bodyUnsanitized, string parentId, string puzzleId, bool deleted)
        {
            ID = id;
            Author = author;
            BodyUnsanitized = bodyUnsanitized;
            ParentID = parentId;
            PuzzleID = puzzleId;
            Deleted = deleted;
        }
    }
}
