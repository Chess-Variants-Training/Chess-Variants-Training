using AtomicChessPuzzles.Models;
using System.Collections.Generic;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface IRatingRepository
    {
        void Add(RatingWithMetadata ratingWithMetaData);

        List<RatingWithMetadata> GetFor(string user);
    }
}
