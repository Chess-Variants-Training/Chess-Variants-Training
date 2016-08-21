using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ChessVariantsTraining.Models
{
    public class RatingWithMetadata
    {
        public ObjectId Id { get; set; }

        [BsonElement("rating")]
        public Rating Rating { get; set; }

        [BsonElement("timestampUtc")]
        public DateTime TimestampUtc { get; set; }

        [BsonElement("owner")]
        public int Owner { get; set; }

        [BsonElement("variant")]
        public string Variant { get; set; }

        public RatingWithMetadata(Rating rating, DateTime timestampUtc, int owner, string variant)
        {
            Rating = rating;
            TimestampUtc = timestampUtc;
            Owner = owner;
            Variant = variant;
        }
    }
}
