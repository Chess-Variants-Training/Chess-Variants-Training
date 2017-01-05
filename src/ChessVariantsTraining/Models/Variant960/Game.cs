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

        [BsonIgnore]
        public ChessGame ChessGame { get; set; }

        public Game() { }

        public Game(GamePlayer white, GamePlayer black, string variant, string fen)
        {
            White = white;
            Black = black;
            Outcome = Outcomes.ONGOING;
            switch (variant)
            {
                case Variants.ANTICHESS960ASYMMETRICAL:
                case Variants.ANTICHESS960SYMMETRICAL:
                    ChessGame = new AntichessGame(fen);
                    break;
                case Variants.ATOMIC960ASYMMETRICAL:
                case Variants.ATOMIC960SYMMETRICAL:
                    ChessGame = new AtomicChessGame(fen);
                    break;
                case Variants.HORDE960:
                    ChessGame = new HordeChessGame(fen);
                    break;
                case Variants.KOTH960ASYMMETRICAL:
                case Variants.KOTH960SYMMETRICAL:
                    ChessGame = new KingOfTheHillChessGame(fen);
                    break;
                case Variants.RK1440ASYMMETRICAL:
                case Variants.RK1440SYMMETRICAL:
                    ChessGame = new RacingKingsChessGame(fen);
                    break;
                case Variants.THREECHECK960ASYMMETRICAL:
                case Variants.THREECHECK960SYMMETRICAL:
                    ChessGame = new ThreeCheckChessGame(fen);
                    break;
                default:
                    throw new InvalidOperationException("Game constructor: invalid variant '" + variant + "'");
            }
        }
    }
}
