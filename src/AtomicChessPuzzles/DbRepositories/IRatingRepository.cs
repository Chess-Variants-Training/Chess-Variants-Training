using AtomicChessPuzzles.Models;
using System;
using System.Collections.Generic;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface IRatingRepository
    {
        void Add(RatingWithMetadata ratingWithMetaData);

        List<RatingWithMetadata> GetFor(string user);

        List<RatingWithMetadata> GetForUserOnRange(string user, DateTime from, DateTime to);
    }
}
