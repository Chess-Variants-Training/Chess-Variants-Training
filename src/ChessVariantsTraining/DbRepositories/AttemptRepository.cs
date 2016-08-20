using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using MongoDB.Driver;

namespace ChessVariantsTraining.DbRepositories
{
    public class AttemptRepository : IAttemptRepository
    {
        MongoSettings settings;
        IMongoCollection<Attempt> attemptCollection;

        public AttemptRepository(ISettings appSettings)
        {
            settings = appSettings.Mongo;
            GetCollection();
        }

        void GetCollection()
        {
            MongoClient client = new MongoClient();
            attemptCollection = client.GetDatabase(settings.Database).GetCollection<Attempt>(settings.AttemptCollectionName);
        }

        public void Add(Attempt attempt)
        {
            attemptCollection.InsertOne(attempt);
        }
    }
}
