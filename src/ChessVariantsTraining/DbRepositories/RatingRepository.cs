using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public class RatingRepository : IRatingRepository
    {
        MongoSettings settings;
        IMongoCollection<RatingWithMetadata> ratingCollection;

        public RatingRepository(IOptions<Settings> appSettings)
        {
            settings = appSettings.Value.Mongo;
            GetCollection();
        }

        void GetCollection()
        {
            MongoClient client = new MongoClient();
            ratingCollection = client.GetDatabase(settings.Database).GetCollection<RatingWithMetadata>(settings.RatingCollectionName);
        }

        public async Task AddAsync(RatingWithMetadata ratingWithMetadata)
        {
            await ratingCollection.InsertOneAsync(ratingWithMetadata);
        }

        public async Task<List<RatingWithMetadata>> GetAsync(int user, DateTime? from, DateTime? to, string show)
        {
            FilterDefinitionBuilder<RatingWithMetadata> builder = Builders<RatingWithMetadata>.Filter;
            FilterDefinition<RatingWithMetadata> filter = builder.Eq("owner", user);
            if (from.HasValue && to.HasValue)
            {
                filter &= builder.Lte("timestampUtc", to.Value) & builder.Gte("timestampUtc", from.Value);
            }
            var found = await ratingCollection.Find(filter).ToListAsync();
            if (show == "each")
            {
                return found;
            }
            else
            {
                var groups = found.GroupBy(x => new { timestamp = x.TimestampUtc.Date, variant = x.Variant });
                Func<RatingWithMetadata, RatingWithMetadata, RatingWithMetadata> bestOfADayAggregator = (agg, next) => next.Rating.Value > agg.Rating.Value ? next : agg;
                Func<RatingWithMetadata, RatingWithMetadata, RatingWithMetadata> endOfTheDayAggregator = (agg, next) => next.TimestampUtc > agg.TimestampUtc ? next : agg;
                Func<RatingWithMetadata, RatingWithMetadata, RatingWithMetadata> aggregator = show == "bestDay" ? bestOfADayAggregator : endOfTheDayAggregator;
                List<RatingWithMetadata> result = groups.Select(x => x.Aggregate(aggregator)).ToList();
                for (int i = 0; i < result.Count; i++)
                {
                    result[i].TimestampUtc = result[i].TimestampUtc.Date;
                }
                return result;
            }
        }
    }
}
