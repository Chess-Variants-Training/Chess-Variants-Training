using ChessDotNet;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ChessVariantsTraining.Services
{
    public class MoveCollectionTransformer : IMoveCollectionTransformer
    {
        public Dictionary<string, List<string>> GetChessgroundDestsForMoveCollection(ReadOnlyCollection<Move> moves)
        {
            Dictionary<string, List<string>> dests = new Dictionary<string, List<string>>();
            foreach (Move m in moves)
            {
                string origin = m.OriginalPosition.ToString().ToLowerInvariant();
                string destination = m.NewPosition.ToString().ToLowerInvariant();
                if (!dests.ContainsKey(origin))
                {
                    dests.Add(origin, new List<string>());
                }
                dests[origin].Add(destination);
            }
            return dests;
        }
    }
}