using ChessVariantsTraining.Models;
using System.Collections.Generic;

namespace ChessVariantsTraining.DbRepositories
{
    public interface ITimedTrainingScoreRepository
    {
        bool Add(TimedTrainingScore score);

        List<TimedTrainingScore> GetLatestScores(int owner);
    }
}
