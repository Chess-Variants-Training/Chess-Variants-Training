using System;
using System.Threading.Tasks;

namespace ChessVariantsTraining.Services
{
    public interface IRatingUpdater
    {
        Task<int?> AdjustRatingAsync(int userId, int puzzleId, bool correct, DateTime attemptStarted, DateTime attemptEnded, string variant);
    }
}