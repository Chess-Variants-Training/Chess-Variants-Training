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

            Dictionary<string, int> pocket = new Dictionary<string, int>();
            foreach (Piece p in zhCurrent.WhitePocket)
            {
                string key = "white-" + p.GetType().Name.ToLowerInvariant();
                if (!pocket.ContainsKey(key))
                {
                    pocket.Add(key, 1);
                }
                else
                {
                    pocket[key]++;
                }
            }

            foreach (Piece p in zhCurrent.BlackPocket)
            {
                string key = "black-" + p.GetType().Name.ToLowerInvariant();
                if (!pocket.ContainsKey(key))
                {
                    pocket.Add(key, 1);
                }
                else
                {
                    pocket[key]++;
                }
            }

            return pocket;
        }
    }
}
