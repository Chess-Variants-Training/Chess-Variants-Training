namespace ChessVariantsTraining.ViewModels
{
    public class Game
    {
        public string White
        {
            get;
            private set;
        }

        public string Black
        {
            get;
            private set;
        }

        public string WhiteUrl
        {
            get;
            private set;
        }

        public string BlackUrl
        {
            get;
            private set;
        }

        public string Variant
        {
            get;
            private set;
        }

        public string TimeControl
        {
            get;
            private set;
        }

        public Game(string white, string black, string whiteUrl, string blackUrl, string variant, string timeControl)
        {
            White = white;
            Black = black;
            WhiteUrl = whiteUrl;
            BlackUrl = blackUrl;
            Variant = variant;
            TimeControl = timeControl;
        }
    }
}
