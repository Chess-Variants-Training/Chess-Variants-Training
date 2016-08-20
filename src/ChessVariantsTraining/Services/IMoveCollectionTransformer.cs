using ChessDotNet;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ChessVariantsTraining.Services
{
    public interface IMoveCollectionTransformer
    {
        Dictionary<string, List<string>> GetChessgroundDestsForMoveCollection(ReadOnlyCollection<Move> moves);
    }
}