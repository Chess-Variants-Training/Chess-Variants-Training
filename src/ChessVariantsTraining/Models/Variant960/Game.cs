using ChessDotNet;
using ChessDotNet.Variants.Antichess;
using ChessDotNet.Variants.Atomic;
using ChessDotNet.Variants.Horde;
using ChessDotNet.Variants.KingOfTheHill;
using ChessDotNet.Variants.RacingKings;
using ChessDotNet.Variants.ThreeCheck;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ChessVariantsTraining.Models.Variant960
{
    public class Game
    {
        public static class Outcomes
        {
            public const string WHITE_WINS = "WhiteWins";
            public const string BLACK_WINS = "BlackWins";
            public const string DRAW = "Draw";
            public const string ONGOING = "Ongoing";
            public const string ABORTED = "Aborted";
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

        [BsonElement("outcome")]
        public string Outcome { get; set; }

        [BsonElement("variant")]
        public string Variant { get; set; }

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

        [BsonIgnore]
        public ChessGame ChessGame { get; set; }

        public Game() { }

        public Game(string id, GamePlayer white, GamePlayer black, string variant, int nWhite, int nBlack)
        {
            ID = id;
            White = white;
            PositionWhite = nWhite;
            Black = black;
            PositionBlack = nBlack;
            Outcome = Outcomes.ONGOING;
            switch (variant)
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
                    throw new InvalidOperationException("Game constructor: invalid variant '" + variant + "'");
            }
            InitialFEN = LatestFEN = ChessGame.GetFen();
        }
    }
}
