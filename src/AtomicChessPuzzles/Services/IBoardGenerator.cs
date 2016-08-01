using ChessDotNet;

namespace AtomicChessPuzzles.Services
{
    public interface IBoardGenerator
    {
        Piece[][] Generate(Piece[] required, bool adjacentKings);
    }
}
