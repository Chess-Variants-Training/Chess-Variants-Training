using ChessDotNet;
using ChessDotNet.Variants.Antichess;
using ChessDotNet.Variants.Atomic;
using ChessDotNet.Variants.Horde;
using ChessDotNet.Variants.KingOfTheHill;
using ChessDotNet.Variants.RacingKings;
using ChessDotNet.Variants.ThreeCheck;
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
        }

        public static class Variants
        {
            public const string ANTICHESS960SYMMETRICAL = "Antichess 960 (symmetrical)";
            public const string ANTICHESS960ASYMMETRICAL = "Antichess 960 (asymmetrical)";
            public const string ATOMIC960SYMMETRICAL = "Atomic 960 (symmetrical)";
            public const string ATOMIC960ASYMMETRICAL = "Atomic 960 (asymmetrical)";
            public const string HORDE960 = "Horde 960";
            public const string KOTH960SYMMETRICAL = "King of the Hill 960 (symmetrical)";
            public const string KOTH960ASYMMETRICAL = "King of the Hill 960 (asymmetrical)";
            public const string RK1440SYMMETRICAL = "Racing Kings 1440 (symmetrical)";
            public const string RK1440ASYMMETRICAL = "Racing Kings 1440 (asymmetrical)";
            public const string THREECHECK960SYMMETRICAL = "Three-check 960 (symmetrical)";
            public const string THREECHECK960ASYMMETRICAL = "Three-check 960 (asymmetrical)";
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

        [BsonElement("moveTimestampsUtc")]
        public List<DateTime> MoveTimeStampsUtc { get; set; }

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

        [BsonIgnore]
        public ChessGame ChessGame { get; set; }

        public Game() { }

        public Game(string id, GamePlayer white, GamePlayer black, string shortVariant, string fullVariant, int nWhite, int nBlack, bool isSymmetrical, TimeControl tc, DateTime startedUtc, int rematchLevel)
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
            switch (fullVariant)
            {
                case Variants.ANTICHESS960ASYMMETRICAL:
                    ChessGame = new AntichessGame(ChessUtilities.FenForChess960Asymmetrical(nWhite, nBlack));
                    break;
                case Variants.ANTICHESS960SYMMETRICAL:
                    ChessGame = new AntichessGame(ChessUtilities.FenForChess960Symmetrical(nWhite));
                    break;
                case Variants.ATOMIC960ASYMMETRICAL:
                    ChessGame = new AtomicChessGame(ChessUtilities.FenForChess960Asymmetrical(nWhite, nBlack));
                    break;
                case Variants.ATOMIC960SYMMETRICAL:
                    ChessGame = new AtomicChessGame(ChessUtilities.FenForChess960Symmetrical(nWhite));
                    break;
                case Variants.HORDE960:
                    ChessGame = new HordeChessGame(ChessUtilities.FenForHorde960(nWhite));
                    break;
                case Variants.KOTH960ASYMMETRICAL:
                    ChessGame = new KingOfTheHillChessGame(ChessUtilities.FenForChess960Asymmetrical(nWhite, nBlack));
                    break;
                case Variants.KOTH960SYMMETRICAL:
                    ChessGame = new KingOfTheHillChessGame(ChessUtilities.FenForChess960Symmetrical(nWhite));
                    break;
                case Variants.RK1440ASYMMETRICAL:
                    ChessGame = new RacingKingsChessGame(ChessUtilities.FenForRacingKings1440Asymmetrical(nWhite, nBlack));
                    break;
                case Variants.RK1440SYMMETRICAL:
                    ChessGame = new RacingKingsChessGame(ChessUtilities.FenForRacingKings1440Symmetrical(nWhite));
                    break;
                case Variants.THREECHECK960ASYMMETRICAL:
                    ChessGame = new ThreeCheckChessGame(ChessUtilities.FenForChess960Asymmetrical(nWhite, nBlack));
                    break;
                case Variants.THREECHECK960SYMMETRICAL:
                    ChessGame = new ThreeCheckChessGame(ChessUtilities.FenForChess960Symmetrical(nWhite));
                    break;
                default:
                    throw new InvalidOperationException("Game constructor: invalid variant '" + fullVariant + "'");
            }
            InitialFEN = LatestFEN = ChessGame.GetFen();
            PlayerChats = new List<ChatMessage>();
            SpectatorChats = new List<ChatMessage>();
            StartedUtc = startedUtc;
            EndedUtc = null;
            ClockWhite = new Clock(tc);
            ClockBlack = new Clock(tc);
            MoveTimeStampsUtc = new List<DateTime>();
            WhiteWantsRematch = false;
            BlackWantsRematch = false;
            RematchLevel = rematchLevel;
        }
    }
}
