using Newtonsoft.Json;
using System.Collections.Generic;


namespace ChessVariantsTraining.Models.Variant960.SocketMessages
{
    public class GameSocketMessage
    {
        public bool Okay { get; protected set; }
        public string Type { get; protected set; }
        public Dictionary<string, object> DeserializedDictionary { get; protected set; }

        protected GameSocketMessage() { }

        public GameSocketMessage(string json)
        {
            try
            {
                DeserializedDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                Okay = true;
            }
            catch
            {
                Okay = false;
            }

            if (Okay)
            {
                if (DeserializedDictionary.ContainsKey("t"))
                {
                    Type = DeserializedDictionary["t"] as string;
                    if (Type == null)
                    {
                        Okay = false;
                    }
                }
                else
                {
                    Okay = false;
                }
            }
        }
    }
}
