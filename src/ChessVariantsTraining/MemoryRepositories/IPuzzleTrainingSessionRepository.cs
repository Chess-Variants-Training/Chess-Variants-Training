using ChessVariantsTraining.Models;

namespace ChessVariantsTraining.MemoryRepositories
{
    public interface IPuzzleTrainingSessionRepository
    {
        void Add(PuzzleTrainingSession session);

        PuzzleTrainingSession Get(string sessionId);

        void Remove(string sessionId);

        bool ContainsTrainingSessionId(string sessionId);
    }
}
