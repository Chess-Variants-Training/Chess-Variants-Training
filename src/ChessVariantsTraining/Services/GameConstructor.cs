using ChessDotNet;
using ChessDotNet.Variants.Atomic;
using System;

namespace ChessVariantsTraining.Services
{
    public class GameConstructor : IGameConstructor
    {
        public ChessGame Construct(string variant)
        {
            switch (variant)
            {
                case "Atomic":
                    return new AtomicChessGame();
                default:
                    throw new NotImplementedException("Variant not implemented: " + variant);
            }
        }

        public ChessGame Construct(string variant, string fen)
        {
            switch (variant)
            {
                case "Atomic":
                    return new AtomicChessGame(fen);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
