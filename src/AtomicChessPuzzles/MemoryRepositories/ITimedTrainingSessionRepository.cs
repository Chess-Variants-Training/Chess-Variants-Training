using AtomicChessPuzzles.Models;
using ChessDotNet.Variants.Atomic;

namespace AtomicChessPuzzles.MemoryRepositories
{
    public interface ITimedTrainingSessionRepository
    {
        void Add(TimedTrainingSession session);

        TimedTrainingSession Get(string sessionId);

        void Remove(string sessionId);

        void SetCurrentFen(string sessionId, string fen, AtomicChessGame associatedGame);
    }
}