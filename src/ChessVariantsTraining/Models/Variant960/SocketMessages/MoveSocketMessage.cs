namespace ChessVariantsTraining.Models.Variant960.SocketMessages
{
    public class MoveSocketMessage : GameSocketMessage
    {
        public string Move { get; private set; }

        public MoveSocketMessage(GameSocketMessage gms) : base()
        {
            Okay = gms.Okay;
            if (!Okay) return;

            DeserializedDictionary = gms.DeserializedDictionary;
            if (DeserializedDictionary.ContainsKey("d"))
            {
                Move = DeserializedDictionary["d"] as string;
            }
            else
            {
                Okay = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(Move))
            {
                Okay = false;
            }
        }
    }
}
