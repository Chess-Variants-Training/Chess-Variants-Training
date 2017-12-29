using MongoDB.Bson.Serialization.Attributes;

namespace ChessVariantsTraining.Models.Variant960
{
    public class AnonymousPlayer : GamePlayer
    {
        [BsonElement("anonymousIdentifier")]
        public string AnonymousIdentifier
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GamePlayer))
            {
                return false;
            }
            return Equals(obj as GamePlayer);
        }

        public override int GetHashCode()
        {
            return new { Type = "Anonymous", AnonymousIdentifier }.GetHashCode();
        }

        public override bool Equals(GamePlayer other)
        {
            if (!(other is AnonymousPlayer))
            {
                return false;
            }
            return (other as AnonymousPlayer).AnonymousIdentifier == AnonymousIdentifier;
        }
    }
}
