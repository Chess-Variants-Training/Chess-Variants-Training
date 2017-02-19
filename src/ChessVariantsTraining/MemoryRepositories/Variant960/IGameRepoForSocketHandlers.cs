using ChessDotNet;
using ChessVariantsTraining.Models.Variant960;

namespace ChessVariantsTraining.MemoryRepositories.Variant960
{
    public interface IGameRepoForSocketHandlers
    {
        Game Get(string id);

        void RegisterMove(Game subject, Move move);

        void RegisterGameOutcome(Game subject, string outcome);
    }
}
