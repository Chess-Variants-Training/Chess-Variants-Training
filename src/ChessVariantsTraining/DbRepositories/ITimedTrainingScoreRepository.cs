using ChessVariantsTraining.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface ITimedTrainingScoreRepository
    {
        bool Add(TimedTrainingScore score);
        List<TimedTrainingScore> GetLatestScores(int owner, string type);
        List<TimedTrainingScore> Get(int userId, DateTime? from, DateTime? to, string show);

        Task<bool> AddAsync(TimedTrainingScore score);
        Task<List<TimedTrainingScore>> GetLatestScoresAsync(int owner, string type);
        Task<List<TimedTrainingScore>> GetAsync(int userId, DateTime? from, DateTime? to, string show);
    }
}
