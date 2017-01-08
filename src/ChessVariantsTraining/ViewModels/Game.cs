using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChessVariantsTraining.ViewModels
{
    public class Game
    {
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

        public Game(string whiteUsername, string blackUsername, int? whiteId, int? blackId, string variant, string timeControl)
        {
            WhiteUsername = whiteUsername;
            BlackUsername = blackUsername;
            WhiteId = whiteId;
            BlackId = blackId;
            Variant = variant;
            TimeControl = timeControl;
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
