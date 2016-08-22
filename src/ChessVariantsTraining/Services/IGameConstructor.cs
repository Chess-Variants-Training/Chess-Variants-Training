using ChessDotNet;

namespace ChessVariantsTraining.Services
{
    public interface IGameConstructor
    {
        ChessGame Construct(string variant);
        ChessGame Construct(string variant, string fen);
    }
}
