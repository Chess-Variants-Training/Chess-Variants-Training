using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

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

        public List<TimedTrainingScore> GetLatestScores(int owner)
        {
            return scoreCollection.Find(Builders<TimedTrainingScore>.Filter.Eq("owner", owner))
                                  .Sort(Builders<TimedTrainingScore>.Sort.Descending("dateRecorded"))
                                  .Limit(15)
                                  .ToList();
        }
    }
}