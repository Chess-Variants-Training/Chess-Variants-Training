using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.MemoryRepositories
{
    public interface IPuzzleTrainingSessionRepository
    {
        void Add(PuzzleTrainingSession session);

        PuzzleTrainingSession Get(string sessionId);

        void Remove(string sessionId);

        bool ContainsTrainingSessionId(string sessionId);
    }
}
