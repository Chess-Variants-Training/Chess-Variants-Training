using System;

namespace AtomicChessPuzzles.Services
{
    public interface IRatingUpdater
    {
        void AdjustRating(string userId, string puzzleId, bool correct, DateTime attemptStarted, DateTime attemptEnded);
    }
}