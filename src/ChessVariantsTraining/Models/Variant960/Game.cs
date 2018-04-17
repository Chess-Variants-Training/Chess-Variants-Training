using ChessDotNet;
using ChessVariantsTraining.Services;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace ChessVariantsTraining.Models.Variant960
{
    public class Game
    {
        public static class Results
        {
            public const string WHITE_WINS = "WhiteWins";
            public const string BLACK_WINS = "BlackWins";
            public const string DRAW = "Draw";
            public const string ONGOING = "Ongoing";
            public const string ABORTED = "Aborted";

            public static string ToFriendlyString(string result)
            {
                if (result == WHITE_WINS)
                {
                    return "1-0, white wins";
                }
                else if (result == BLACK_WINS)
                {
                    return "0-1, black wins";
                }
                else if (result == DRAW)
                {
                    return "½-½, draw";
                }
                else
                {
                    return result;
                }
            }
        }

        public static class Terminations
        {
            public const string NORMAL = "Normal";
            public const string RESIGNATION = "Resignation";
            public const string TIME_FORFEIT = "Time forfeit";
            public const string RULES_INFRACTION = "Rules infraction";
            public const string ABANDONED = "Abandoned";
            public const string UNTERMINATED = "Unterminated";
            public const string ADJUDICATION = "Adjudication";
            public const string ABORTED = "Aborted";
        }

        [BsonElement("_id")]
        public string ID { get; set; }

        [BsonElement("white")]
        public GamePlayer White { get; set; }

        [BsonElement("black")]
        public GamePlayer Black { get; set; }

        [BsonElement("result")]
        public string Result { get; set; }

        [BsonElement("termination")]
        public string Termination { get; set; }

        [BsonElement("shortVariantName")]
        public string ShortVariantName { get; set; }

        [BsonElement("fullVariantName")]
        public string FullVariantName { get; set; }

        [BsonElement("pgn")]
        public string PGN { get; set; }

        [BsonElement("initialFen")]
        public string InitialFEN { get; set; }

        [BsonElement("latestFen")]
        public string LatestFEN { get; set; }

        [BsonElement("positionWhite")]
        public int PositionWhite { get; set; }

        [BsonElement("positionBlack")]
        public int PositionBlack { get; set; }

        [BsonElement("timeControl")]
        public TimeControl TimeControl { get; set; }

        [BsonElement("playerChats")]
        public List<ChatMessage> PlayerChats { get; set; }

        [BsonElement("spectatorChats")]
        public List<ChatMessage> SpectatorChats { get; set; }

        [BsonElement("startedUtc")]
        public DateTime StartedUtc { get; set; }

        [BsonElement("endedUtc")]
        public DateTime? EndedUtc { get; set; }

        [BsonElement("clockWhite")]
        public Clock ClockWhite { get; set; }

        [BsonElement("clockBlack")]
        public Clock ClockBlack { get; set; }

        [BsonElement("clockTimes")]
        public List<double> ClockTimes { get; set; }

        [BsonElement("whiteWantsRematch")]
        public bool WhiteWantsRematch { get; set; }

        [BsonElement("blackWantsRematch")]
        public bool BlackWantsRematch { get; set; }

        [BsonElement("rematchId")]
        public string RematchID { get; set; }

        [BsonElement("rematchLevel")]
        public int RematchLevel { get; set; }

        [BsonElement("isSymmetrical")]
        public bool IsSymmetrical { get; set; }

        [BsonElement("uciMoves")]
        public List<string> UciMoves { get; set; }

        [BsonElement("whiteWantsDraw")]
        public bool WhiteWantsDraw { get; set; }

        [BsonElement("blackWantsDraw")]
        public bool BlackWantsDraw { get; set; }

        [BsonIgnore]
        public ChessGame ChessGame { get; set; }

        public Game() { }

        public Game(string id, GamePlayer white, GamePlayer black, string shortVariant, string fullVariant, int nWhite, int nBlack, bool isSymmetrical, TimeControl tc, DateTime startedUtc, int rematchLevel, IGameConstructor gameConstructor)
        {
            ID = id;
            White = white;
            PositionWhite = nWhite;
            Black = black;
            PositionBlack = nBlack;
            IsSymmetrical = isSymmetrical;
            Result = Results.ONGOING;
            Termination = Terminations.UNTERMINATED;
            TimeControl = tc;
            ShortVariantName = shortVariant;
            FullVariantName = fullVariant;
            string fen;
            if (shortVariant == "Horde")
            {
                fen = ChessUtilities.FenForHorde960(nBlack);
            }
            else if (shortVariant == "RacingKings")
            {
                fen = ChessUtilities.FenForRacingKings1440Asymmetrical(nWhite, nBlack);
            }
            else
            {
                fen = ChessUtilities.FenForChess960Asymmetrical(nWhite, nBlack);
            }
            ChessGame = gameConstructor.Construct(shortVariant, fen);
            InitialFEN = LatestFEN = ChessGame.GetFen();
            PlayerChats = new List<ChatMessage>();
            SpectatorChats = new List<ChatMessage>();
            StartedUtc = startedUtc;
            EndedUtc = null;
            ClockWhite = new Clock(tc);
            ClockBlack = new Clock(tc);
            ClockTimes = new List<double>();
            WhiteWantsRematch = false;
            BlackWantsRematch = false;
            WhiteWantsDraw = false;
            BlackWantsDraw = false;
            RematchLevel = rematchLevel;
            UciMoves = new List<string>();
        }
    }
}
