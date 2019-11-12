using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessVariantsTraining.Models
{
    public class PuzzleTag
    {
        public ObjectId Id { get; set; }

        [BsonElement("variant")]
        public string Variant { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        [BsonDefaultValue(null)]
        public string Description { get; set; }
    }
}
