using System;

namespace ChessVariantsTraining.Services
{
    public interface IRatingUpdater
    {
        void AdjustRating(int userId, int puzzleId, bool correct, DateTime attemptStarted, DateTime attemptEnded);
    }
}