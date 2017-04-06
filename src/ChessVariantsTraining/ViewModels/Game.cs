using ChessDotNet;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChessVariantsTraining.ViewModels
{
    public class Game
    {
        public string GameID
        {
            get;
            private set;
        }

        public string WhiteUsername
        {
            get;
            private set;
        }

        public string BlackUsername
        {
            get;
            private set;
        }

        public int? WhiteId
        {
            get;
            private set;
        }

        public int? BlackId
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

        public string FEN
        {
            get;
            private set;
        }

        public bool IsPlayer
        {
            get;
            private set;
        }

        public string MyColor
        {
            get;
            private set;
        }

        public string WhoseTurn
        {
            get;
            private set;
        }

        public bool IsFinished
        {
            get;
            private set;
        }

        public string DestsJSON
        {
            get;
            private set;
        }

        public string ShortVariant
        {
            get;
            private set;
        }

        public string Result
        {
            get;
            private set;
        }

        public string Termination
        {
            get;
            private set;
        }

        public Game(string gameId, string whiteUsername, string blackUsername, int? whiteId, int? blackId, string shortVariant, string variant, string timeControl, string fen, bool isPlayer, string myColor, string whoseTurn, bool isFinished, string destsJson, string result, string termination)
        {
            GameID = gameId;
            WhiteUsername = whiteUsername;
            BlackUsername = blackUsername;
            WhiteId = whiteId;
            BlackId = blackId;
            ShortVariant = shortVariant;
            Variant = variant;
            TimeControl = timeControl;
            FEN = fen;
            IsPlayer = isPlayer;
            MyColor = myColor;
            WhoseTurn = whoseTurn;
            IsFinished = isFinished;
            DestsJSON = destsJson;
            Result = result;
            Termination = termination;
        }

        public HtmlString RenderWhiteLink(IUrlHelper helper)
        {
            if (WhiteId.HasValue)
            {
                return new HtmlString(string.Format("<a href='{0}'>{1}</a>", helper.Action("Profile", "User", new { id = WhiteId.Value }), WhiteUsername));
            }
            else
            {
                return new HtmlString("Anonymous");
            }
        }

        public HtmlString RenderBlackLink(IUrlHelper helper)
        {
            if (BlackId.HasValue)
            {
                return new HtmlString(string.Format("<a href='{0}'>{1}</a>", helper.Action("Profile", "User", new { id = BlackId.Value }), BlackUsername));
            }
            else
            {
                return new HtmlString("Anonymous");
            }
        }
    }
}
