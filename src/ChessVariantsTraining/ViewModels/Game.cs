using ChessVariantsTraining.Models.Variant960;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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

        public TimeControl TimeControl
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

        public string LastMove
        {
            get;
            private set;
        }

        public string Check
        {
            get;
            private set;
        }

        public int Plies
        {
            get;
            private set;
        }

        public bool WhiteWantsDraw
        {
            get;
            private set;
        }

        public bool BlackWantsDraw
        {
            get;
            private set;
        }

        public bool WhiteWantsRematch
        {
            get;
            private set;
        }

        public bool BlackWantsRematch
        {
            get;
            private set;
        }

        public List<string> ReplayFENs
        {
            get;
            private set;
        }

        public List<string> ReplayMoves
        {
            get;
            private set;
        }

        public List<string> ReplayChecks
        {
            get;
            private set;
        }

        public Dictionary<string, int> ZhPocket
        {
            get;
            private set;
        }

        public List<Dictionary<string, int>> ReplayPocket
        {
            get;
            private set;
        }

        public int WhitePosition
        {
            get;
            private set;
        }

        public int BlackPosition
        {
            get;
            private set;
        }

        public string PGN
        {
            get;
            private set;
        }

        public string InitialFEN
        {
            get;
            private set;
        }

        public Game(string gameId,
            string whiteUsername,
            string blackUsername,
            int? whiteId,
            int? blackId,
            string shortVariant,
            string variant,
            TimeControl timeControl,
            string fen,
            bool isPlayer,
            string myColor,
            string whoseTurn,
            bool isFinished,
            string destsJson,
            string result,
            string termination,
            string lastMove,
            string check,
            int plies,
            bool whiteWantsDraw,
            bool blackWantsDraw,
            bool whiteWantsRematch,
            bool blackWantsRematch,
            List<string> replayFens,
            List<string> replayMoves,
            List<string> replayChecks,
            Dictionary<string, int> zhPocket,
            List<Dictionary<string, int>> replayPocket,
            int whitePosition,
            int blackPosition,
            string pgn,
            string initialFen)
        {
            GameID = gameId;
            WhiteUsername = whiteUsername;
            BlackUsername = blackUsername;
            WhiteId = whiteId;
            BlackId = blackId;
            ShortVariant = shortVariant;
            Variant = variant.Split(new string[] { " (" }, StringSplitOptions.None)[0];
            TimeControl = timeControl;
            FEN = fen;
            IsPlayer = isPlayer;
            MyColor = myColor;
            WhoseTurn = whoseTurn;
            IsFinished = isFinished;
            DestsJSON = destsJson;
            Result = result;
            Termination = termination;
            LastMove = lastMove;
            Check = check;
            Plies = plies;
            WhiteWantsDraw = whiteWantsDraw;
            BlackWantsDraw = blackWantsDraw;
            WhiteWantsRematch = whiteWantsRematch;
            BlackWantsRematch = blackWantsRematch;
            ReplayFENs = replayFens;
            ReplayMoves = replayMoves;
            ReplayChecks = replayChecks;
            ZhPocket = zhPocket;
            ReplayPocket = replayPocket;
            WhitePosition = whitePosition;
            BlackPosition = blackPosition;
            PGN = pgn;
            InitialFEN = initialFen;
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

        public HtmlString RenderLastMoveAsArray()
        {
            if (LastMove == null)
            {
                return new HtmlString("null");
            }
            else
            {
                return new HtmlString(string.Format("[\"{0}\",\"{1}\"]", LastMove.Substring(0, 2), LastMove.Substring(2, 2)));
            }
        }

        public HtmlString RenderCheckingSquare()
        {
            if (Check == null)
            {
                return new HtmlString("null");
            }
            else
            {
                return new HtmlString("\"" + Check + "\"");
            }
        }

        public HtmlString RenderWhiteText()
        {
            if (IsPlayer && MyColor == "white")
            {
                return new HtmlString("White (you):");
            }
            else
            {
                return new HtmlString("White:");
            }
        }

        public HtmlString RenderBlackText()
        {
            if (IsPlayer && MyColor == "black")
            {
                return new HtmlString("Black (you):");
            }
            else
            {
                return new HtmlString("Black:");
            }
        }

        public HtmlString RenderReplayFens()
        {
            return new HtmlString(string.Format("['{0}']", string.Join("','", ReplayFENs)));
        }
        
        public HtmlString RenderReplayMoves()
        {
            if (ReplayMoves.Count > 0)
            {
                return new HtmlString(string.Format("[null,'{0}']", string.Join("','", ReplayMoves)));
            }
            else
            {
                return new HtmlString("[null]");
            }
        }

        public HtmlString RenderReplayChecks()
        {
            List<string> stringifiedReplayChecks = new List<string>();
            foreach (string replayCheck in ReplayChecks)
            {
                if (replayCheck == null)
                {
                    stringifiedReplayChecks.Add("null");
                }
                else
                {
                    stringifiedReplayChecks.Add(string.Concat("'", replayCheck, "'"));
                }
            }

            return new HtmlString(string.Format("[{0}]", string.Join(",", stringifiedReplayChecks)));
        }

        public HtmlString RenderZhPocket()
        {
            if (ZhPocket == null)
            {
                return new HtmlString("null");
            }
            else
            {
                return new HtmlString(JsonConvert.SerializeObject(ZhPocket));
            }
        }

        public HtmlString RenderPocketReplay()
        {
            if (ReplayPocket == null)
            {
                return new HtmlString("null");
            }
            else
            {
                return new HtmlString(JsonConvert.SerializeObject(ReplayPocket));
            }
        }
    }
}
