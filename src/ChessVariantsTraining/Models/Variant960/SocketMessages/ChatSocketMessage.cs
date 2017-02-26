namespace ChessVariantsTraining.Models.Variant960.SocketMessages
{
    public class ChatSocketMessage : GameSocketMessage
    {
        public string Content { get; private set; }
        public string Channel { get; private set; }

        public ChatSocketMessage(GameSocketMessage gms) : base()
        {
            Okay = gms.Okay;
            if (!Okay) return;

            Type = gms.Type;
            DeserializedDictionary = gms.DeserializedDictionary;
            if (DeserializedDictionary.ContainsKey("d"))
            {
                Content = DeserializedDictionary["d"] as string;
            }
            else
            {
                Okay = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(Content))
            {
                Okay = false;
                return;
            }

            if (DeserializedDictionary.ContainsKey("channel"))
            {
                Channel = DeserializedDictionary["channel"] as string;
            }
            else
            {
                Okay = false;
                return;
            }

            if (Channel != "player" && Channel != "spectator")
            {
                Okay = false;
                return;
            }
        }
    }
}
