using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AtomicChessPuzzles.Models
{
    public class RatingWithMetadata
    {
        public ObjectId Id { get; set; }

        [BsonElement("rating")]
        public Rating Rating { get; set; }

        [BsonElement("timestampUtc")]
        public DateTime TimestampUtc { get; set; }

        [BsonElement("owner")]
        public string Owner { get; set; }

        public RatingWithMetadata(Rating rating, DateTime timestampUtc, string owner)
        {
            Rating = rating;
            TimestampUtc = timestampUtc;
            Owner = owner;
        }
    }
}
