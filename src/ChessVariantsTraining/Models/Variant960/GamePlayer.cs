using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ChessVariantsTraining.Models.Variant960
{
    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(AnonymousPlayer), typeof(RegisteredPlayer))]
    public abstract class GamePlayer : IEquatable<GamePlayer>
    {
        public abstract bool Equals(GamePlayer other);
    }
}
