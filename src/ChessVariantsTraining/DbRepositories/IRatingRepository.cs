using ChessVariantsTraining.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface IRatingRepository
    {
        void Add(RatingWithMetadata ratingWithMetaData);
        List<RatingWithMetadata> Get(int user, DateTime? from, DateTime? to, string show);

        Task AddAsync(RatingWithMetadata ratingWithMetaData);
        Task<List<RatingWithMetadata>> GetAsync(int user, DateTime? from, DateTime? to, string show);
    }
}
