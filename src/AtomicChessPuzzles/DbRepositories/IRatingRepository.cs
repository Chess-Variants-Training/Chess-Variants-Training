using AtomicChessPuzzles.Models;
using System;
using System.Collections.Generic;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface IRatingRepository
    {
        void Add(RatingWithMetadata ratingWithMetaData);

        List<RatingWithMetadata> Get(string user, DateTime? from, DateTime? to, string show);
    }
}
