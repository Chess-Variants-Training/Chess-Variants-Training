using Newtonsoft.Json;
using System.Collections.Generic;


namespace ChessVariantsTraining.Models.Variant960.SocketMessages
{
    public class GameSocketMessage
    {
        public bool Okay { get; protected set; }
        public string Type { get; protected set; }
        protected Dictionary<string, object> DeserializedDictionary { get; set; }

        protected GameSocketMessage() { }

        public GameSocketMessage(string json)
        {
            Dictionary<string, object> deserialized = null;
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
                if (deserialized.ContainsKey("t"))
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
