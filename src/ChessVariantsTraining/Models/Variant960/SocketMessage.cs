using System.Collections.Generic;
using Newtonsoft.Json;

namespace ChessVariantsTraining.Models.Variant960
{
    public class SocketMessage
    {
        public bool Okay { get; private set; }
        public string Type { get; private set; }
        public string Data { get; private set; }

        public SocketMessage(string json)
        {
            Dictionary<string, string> deserialized = null;
            try
            {
                deserialized = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
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
                    Type = deserialized["t"];
                }
                else
                {
                    Okay = false;
                }
                if (deserialized.ContainsKey("d"))
                {
                    Data = deserialized["d"];
                }
                else
                {
                    Okay = false;
                }
            }
        }
    }
}
