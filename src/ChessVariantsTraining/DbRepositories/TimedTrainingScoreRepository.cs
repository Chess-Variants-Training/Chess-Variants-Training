using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public class TimedTrainingScoreRepository : ITimedTrainingScoreRepository
    {
        MongoSettings settings;
        IMongoCollection<TimedTrainingScore> scoreCollection;

        public TimedTrainingScoreRepository(IOptions<Settings> appSettings)
        {
            settings = appSettings.Value.Mongo;
            GetCollection();
        }

        private void GetCollection()
        {
            MongoClient client = new MongoClient();
            scoreCollection = client.GetDatabase(settings.Database).GetCollection<TimedTrainingScore>(settings.TimedTrainingScoreCollectionName);
        }

        public bool Add(TimedTrainingScore score)
        {
            try
            {
                scoreCollection.InsertOne(score);
            }
            catch (Exception e) when (e is MongoWriteException || e is MongoBulkWriteException)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> AddAsync(TimedTrainingScore score)
        {
            try
            {
                await scoreCollection.InsertOneAsync(score);
            }
            catch (Exception e) when (e is MongoWriteException || e is MongoBulkWriteException)
            {
                return false;
            }
            return true;
        }

        public async Task<List<TimedTrainingScore>> GetLatestScoresAsync(int owner, string type)
        {
            return await scoreCollection.Find(Builders<TimedTrainingScore>.Filter.Eq("owner", owner) & Builders<TimedTrainingScore>.Filter.Eq("type", type))
                                       .Sort(Builders<TimedTrainingScore>.Sort.Descending("dateRecorded"))
                                       .Limit(15)
                                       .ToListAsync();
        }

        public async Task<List<TimedTrainingScore>> GetAsync(int user, DateTime? from, DateTime? to, string show)
        {
            FilterDefinitionBuilder<TimedTrainingScore> builder = Builders<TimedTrainingScore>.Filter;
            FilterDefinition<TimedTrainingScore> filter = builder.Eq("owner", user);
            if (from.HasValue && to.HasValue)
            {
                filter &= builder.Lte("dateRecorded", to.Value) & builder.Gte("dateRecorded", from.Value);
            }
            var found = await scoreCollection.Find(filter).ToListAsync();
            if (show == "each")
            {
                return found;
            }
            else
            {
                var groups = found.GroupBy(x => new { x.DateRecordedUtc.Date, x.Type, x.Variant });
                if (show == "bestDay")
                {
                    Func<TimedTrainingScore, TimedTrainingScore, TimedTrainingScore> bestOfADayAggregator = (agg, next) => next.Score > agg.Score ? next : agg;
                    List<TimedTrainingScore> result = groups.Select(x => x.Aggregate(bestOfADayAggregator)).ToList();
                    for (int i = 0; i < result.Count; i++)
                    {
                        result[i].DateRecordedUtc = result[i].DateRecordedUtc.Date;
                    }
                    return result;
                }
                else // show == "avgDay"
                {
                    return groups.Select(x => new TimedTrainingScore(x.Average(y => y.Score), x.Key.Type, user, x.Key.Date, x.Key.Variant)).ToList();
                }
            }
        }
    }
}