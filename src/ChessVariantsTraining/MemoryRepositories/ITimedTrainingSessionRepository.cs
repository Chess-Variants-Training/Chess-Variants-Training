using ChessVariantsTraining.Models;

namespace ChessVariantsTraining.MemoryRepositories
{
    public interface ITimedTrainingSessionRepository
    {
        void Add(TimedTrainingSession session);

        TimedTrainingSession Get(string sessionId);

        void Remove(string sessionId);
    }
}