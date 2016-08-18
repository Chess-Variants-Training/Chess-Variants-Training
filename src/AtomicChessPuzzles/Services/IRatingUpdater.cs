using System;

namespace AtomicChessPuzzles.Services
{
    public interface IRatingUpdater
    {
        void AdjustRating(int userId, string puzzleId, bool correct, DateTime attemptStarted, DateTime attemptEnded);
    }
}