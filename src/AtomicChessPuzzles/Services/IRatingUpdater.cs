using System;

namespace AtomicChessPuzzles.Services
{
    public interface IRatingUpdater
    {
        void AdjustRating(int userId, int puzzleId, bool correct, DateTime attemptStarted, DateTime attemptEnded);
    }
}