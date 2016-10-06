using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ChessVariantsTraining.Models
{
    public class Comment
    {
        [BsonElement("_id")]
        public int ID { get; set; }

        [BsonElement("author")]
        public int Author { get; set; }

        [BsonElement("bodyUnsanitized")]
        public string Body { get; set; }

        [BsonElement("parentId")]
        public int? ParentID { get; set; }

        [BsonElement("puzzleId")]
        public int PuzzleID { get; set; }

        [BsonElement("deleted")]
        public bool Deleted { get; set; }

        [BsonElement("datePostedUtc")]
        public DateTime DatePostedUtc { get; set; }

        public Comment() { }

        public Comment(int id, int author, string bodyUnsanitized, int? parentId, int puzzleId, bool deleted, DateTime creationDateUtc)
        {
            ID = id;
            Author = author;
            Body = bodyUnsanitized;
            ParentID = parentId;
            PuzzleID = puzzleId;
            Deleted = deleted;
            DatePostedUtc = creationDateUtc;
        }
    }
}
