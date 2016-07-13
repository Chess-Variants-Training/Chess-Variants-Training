using AtomicChessPuzzles.Models;
using MongoDB.Driver;
using System;

namespace AtomicChessPuzzles.DbRepositories
{
    public class TimedTrainingScoreRepository : ITimedTrainingScoreRepository
    {
        MongoSettings settings;
        IMongoCollection<TimedTrainingScore> scoreCollection;

        public TimedTrainingScoreRepository()
        {
            settings = new MongoSettings();
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
    }
}