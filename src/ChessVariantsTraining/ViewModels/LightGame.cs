namespace ChessVariantsTraining.ViewModels
{
    public class LightGame
    {
        public string White { get; private set; }
        public string Black { get; private set; }
        public string Result { get; private set; }
        public string Url { get; private set; }
        public string TimeStarted { get; private set; }

        public LightGame(string white, string black, string result, string url, string timeStarted)
        {
            White = white;
            Black = black;
            Result = result;
            Url = url;
            TimeStarted = timeStarted;
        }
    }
}
