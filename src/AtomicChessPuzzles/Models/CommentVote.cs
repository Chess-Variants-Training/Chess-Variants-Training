using MongoDB.Bson.Serialization.Attributes;

namespace AtomicChessPuzzles.Models
{
    public class CommentVote
    {
        [BsonElement("_id")]
        public string ID { get; private set; }

        [BsonElement("type")]
        public VoteType Type { get; set; }

        [BsonElement("voter")]
        public string Voter { get; set; }

        [BsonElement("affectedComment")]
        public string AffectedComment { get; set; }

        public CommentVote(VoteType type, string voter, string affectedComment)
        {
            voter = voter.ToLowerInvariant();
            ID = voter + ":" + affectedComment;
            Type = type;
            Voter = voter;
            AffectedComment = affectedComment;
        }
    }
}
