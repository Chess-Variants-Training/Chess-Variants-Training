using ChessVariantsTraining.Models;
using System;
using System.Collections.Generic;

namespace ChessVariantsTraining.DbRepositories
{
    public interface IRatingRepository
    {
        void Add(RatingWithMetadata ratingWithMetaData);

        List<RatingWithMetadata> Get(int user, DateTime? from, DateTime? to, string show);
    }
}
