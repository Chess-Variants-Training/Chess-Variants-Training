using System.Collections.Generic;
using ChessDotNet;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ChessVariantsTraining.Models
{
    public class Puzzle
    {
        [BsonIgnore]
        public ChessGame Game
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

        [BsonElement("variant")]
        public string Variant
        {
            get;
            set;
        }

        [BsonElement("dateSubmittedUtc")]
        public DateTime DateSubmittedUtc
        {
            get;
            set;
        }

        [BsonElement("reviewers")]
        public List<int> Reviewers
        {
            get;
            set;
        }

        [BsonElement("tags")]
        public List<string> Tags
        {
            get;
            set;
        }
    }
}
