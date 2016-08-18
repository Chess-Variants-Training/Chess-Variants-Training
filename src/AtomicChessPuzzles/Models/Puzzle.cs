using System.Collections.Generic;
using ChessDotNet.Variants.Atomic;
using MongoDB.Bson.Serialization.Attributes;

namespace AtomicChessPuzzles.Models
{
    public class Puzzle
    {
        [BsonIgnore]
        public AtomicChessGame Game
        {
            get;
            set;
        }

        [BsonElement("solutions")]
        public List<string> Solutions
        {
            get;
            set;
        }

        [BsonElement("_id")]
        public int ID
        {
            get;
            set;
        }

        [BsonElement("initialfen")]
        public string InitialFen
        {
            get;
            set;
        }

        [BsonElement("author")]
        public int Author
        {
            get;
            set;
        }

        [BsonElement("explanation")]
        public string ExplanationUnsafe
        {
            get;
            set;
        }

        [BsonIgnore]
        public string ExplanationSafe
        {
            get
            {
                return Utilities.SanitizeHtml(ExplanationUnsafe ?? "No explanation provided.");
            }
        }

        [BsonElement("rating")]
        public Rating Rating
        {
            get;
            set;
        }

        [BsonElement("inReview")]
        public bool InReview
        {
            get;
            set;
        }

        [BsonElement("approved")]
        public bool Approved
        {
            get;
            set;
        }
    }
}
