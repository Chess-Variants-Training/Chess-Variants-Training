using AtomicChessPuzzles.Models;
using System.Collections.Generic;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface ITimedTrainingScoreRepository
    {
        bool Add(TimedTrainingScore score);

        List<TimedTrainingScore> GetLatestScores(int owner);
    }
}
