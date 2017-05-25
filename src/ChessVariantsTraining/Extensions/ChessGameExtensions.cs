using ChessDotNet;
using ChessDotNet.Variants.Crazyhouse;
using System.Collections.Generic;

namespace ChessVariantsTraining.Extensions
{
    public static class ChessGameExtensions
    {
        public static Dictionary<string, int> GenerateJsonPocket(this ChessGame game)
        {
            CrazyhouseChessGame zhCurrent = game as CrazyhouseChessGame;
            if (zhCurrent == null)
            {
                return null;
            }

            Dictionary<string, int> pocket = new Dictionary<string, int>()
            {
                { "white-queen", 0 },
                { "white-rook", 0 },
                { "white-bishop", 0 },
                { "white-knight", 0 },
                { "white-pawn", 0 },
                { "black-queen", 0 },
                { "black-rook", 0 },
                { "black-bishop", 0 },
                { "black-knight", 0 },
                { "black-pawn", 0 }
            };
            foreach (Piece p in zhCurrent.WhitePocket)
            {
                string key = "white-" + p.GetType().Name.ToLowerInvariant();
                pocket[key]++;
            }

            foreach (Piece p in zhCurrent.BlackPocket)
            {
                string key = "black-" + p.GetType().Name.ToLowerInvariant();
                pocket[key]++;
            }

            return pocket;
        }
    }
}
