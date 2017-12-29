using MongoDB.Bson.Serialization.Attributes;

namespace ChessVariantsTraining.Models.Variant960
{
    public class RegisteredPlayer : GamePlayer
    {
        [BsonElement("userId")]
        public int UserId
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
            return new { Type = "Registered", UserId }.GetHashCode();
        }

        public override bool Equals(GamePlayer other)
        {
            if (!(other is RegisteredPlayer))
            {
                return false;
            }
            return (other as RegisteredPlayer).UserId == UserId;
        }
    }
}
