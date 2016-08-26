using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ChessVariantsTraining.DbRepositories
{
    public class AttemptRepository : IAttemptRepository
    {
        MongoSettings settings;
        IMongoCollection<Attempt> attemptCollection;

        public AttemptRepository(IOptions<Settings> appSettings)
        {
            settings = appSettings.Value.Mongo;
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
