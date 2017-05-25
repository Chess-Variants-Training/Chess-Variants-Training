using ChessDotNet;
using ChessDotNet.Variants.Antichess;
using ChessDotNet.Variants.Atomic;
using ChessDotNet.Variants.Crazyhouse;
using ChessDotNet.Variants.Horde;
using ChessDotNet.Variants.KingOfTheHill;
using ChessDotNet.Variants.RacingKings;
using ChessDotNet.Variants.ThreeCheck;
using System;

namespace ChessVariantsTraining.Services
{
    public class GameConstructor : IGameConstructor
    {
        public ChessGame Construct(string variant)
        {
            switch (variant)
            {
                case "Antichess":
                    return new AntichessGame();
                case "Atomic":
                    return new AtomicChessGame();
                case "Crazyhouse":
                    return new CrazyhouseChessGame();
                case "Horde":
                    return new HordeChessGame();
                case "KingOfTheHill":
                    return new KingOfTheHillChessGame();
                case "RacingKings":
                    return new RacingKingsChessGame();
                case "ThreeCheck":
                    return new ThreeCheckChessGame();
                default:
                    throw new NotImplementedException("Variant not implemented: " + variant);
            }
        }

        public ChessGame Construct(string variant, string fen)
        {
            switch (variant)
            {
                case "Antichess":
                    return new AntichessGame(fen);
                case "Atomic":
                    return new AtomicChessGame(fen);
                case "Crazyhouse":
                    return new CrazyhouseChessGame(fen);
                case "Horde":
                    return new HordeChessGame(fen);
                case "KingOfTheHill":
                    return new KingOfTheHillChessGame(fen);
                case "RacingKings":
                    return new RacingKingsChessGame(fen);
                case "ThreeCheck":
                    return new ThreeCheckChessGame(fen);
                default:
                    throw new NotImplementedException("Variant not implemented: " + variant);
            }
        }
    }
}
