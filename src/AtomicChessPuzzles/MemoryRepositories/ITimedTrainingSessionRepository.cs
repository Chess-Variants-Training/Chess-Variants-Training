using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.MemoryRepositories
{
    public interface ITimedTrainingSessionRepository
    {
        void Add(TimedTrainingSession session);

        TimedTrainingSession Get(string sessionId);

        void Remove(string sessionId);
    }
}