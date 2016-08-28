using ChessVariantsTraining.Models;
using System;
using System.Collections.Generic;

namespace ChessVariantsTraining.DbRepositories
{
    public interface ITimedTrainingScoreRepository
    {
        bool Add(TimedTrainingScore score);

        List<TimedTrainingScore> GetLatestScores(int owner, string type);

        List<TimedTrainingScore> Get(int userId, DateTime? from, DateTime? to, string show);
    }
}
