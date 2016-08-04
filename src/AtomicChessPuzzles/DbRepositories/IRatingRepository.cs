using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface IRatingRepository
    {
        void Add(RatingWithMetadata ratingWithMetaData);
    }
}
