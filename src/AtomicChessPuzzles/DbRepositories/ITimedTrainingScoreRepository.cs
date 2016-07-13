using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface ITimedTrainingScoreRepository
    {
        bool Add(TimedTrainingScore score);
    }
}
