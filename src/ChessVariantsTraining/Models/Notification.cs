using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ChessVariantsTraining.Models
{
    public class Notification
    {
        [BsonElement("_id")]
        public string ID { get; set; }

        [BsonElement("user")]
        public int User { get; set; }

        [BsonElement("content")]
        public string Content { get; set; }

        [BsonElement("read")]
        public bool Read { get; set; }

        [BsonElement("timestampUtc")]
        public DateTime TimestampUtc { get; set; }

        [BsonElement("url")]
        public string URL { get; set; }

        public Notification() { }

        public Notification(string id, int user, string content, bool read, string url, DateTime timestampUtc)
        {
            ID = id;
            User = user;
            Content = content;
            Read = read;
            URL = url;
            TimestampUtc = timestampUtc;
        }
    }
}
