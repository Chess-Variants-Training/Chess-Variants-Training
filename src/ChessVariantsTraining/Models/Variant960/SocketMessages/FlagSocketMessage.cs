using System.Linq;

namespace ChessVariantsTraining.Models.Variant960.SocketMessages
{
    public class FlagSocketMessage : GameSocketMessage
    {
        public string Player { get; private set; }

        public FlagSocketMessage(GameSocketMessage gsm) : base()
        {
            Okay = gsm.Okay;
            if (!Okay) return;

            Type = gsm.Type;
            DeserializedDictionary = gsm.DeserializedDictionary;
            if (!(DeserializedDictionary.ContainsKey("d") && new string[] { "white", "black" }.Contains((Player = DeserializedDictionary["d"] as string))))
            {
                Okay = false;
            }
        }
    }
}
