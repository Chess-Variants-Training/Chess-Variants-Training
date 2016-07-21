namespace AtomicChessPuzzles.Services
{
    public interface IRatingUpdater
    {
        void AdjustRating(string userId, string puzzleId, bool correct);
    }
}