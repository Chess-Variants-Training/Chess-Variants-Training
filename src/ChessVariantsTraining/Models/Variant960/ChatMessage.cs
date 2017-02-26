using MongoDB.Bson.Serialization.Attributes;

namespace ChessVariantsTraining.Models.Variant960
{
    public class ChatMessage
    {
        [BsonElement("userId")]
        public int? UserID { get; set; }

        [BsonElement("displayName")]
        public string DisplayName { get; set; }

        [BsonElement("contentUnescaped")]
        public string ContentUnescaped { get; set; }

        public ChatMessage() { }

        public ChatMessage(int? userId, string displayName, string contentUnescaped)
        {
            UserID = userId;
            DisplayName = displayName;
            ContentUnescaped = contentUnescaped;
        }

        public string GetHtml()
        {
            if (UserID.HasValue)
            {
                return string.Format("<a href='/User/Profile/{0}' target='_blank'>{1}</a> - {2}", UserID.Value, Utilities.SanitizeHtml(DisplayName), Utilities.SanitizeHtml(ContentUnescaped));
            }
            else
            {
                return string.Format("{0} - {1}", Utilities.SanitizeHtml(DisplayName), Utilities.SanitizeHtml(ContentUnescaped));
            }
        }
    }
}
