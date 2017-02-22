using MongoDB.Bson.Serialization.Attributes;

namespace ChessVariantsTraining.Models.Variant960
{
    public class ChatMessage
    {
        [BsonElement("userId")]
        public int? UserID { get; set; }

        [BsonElement("displayName")]
        public string DisplayName { get; set; }

        [BsonElement("content")]
        public string Content { get; set; }

        public ChatMessage() { }

        public ChatMessage(int? userId, string displayName, string content)
        {
            UserID = userId;
            DisplayName = displayName;
            Content = content;
        }
    }
}
