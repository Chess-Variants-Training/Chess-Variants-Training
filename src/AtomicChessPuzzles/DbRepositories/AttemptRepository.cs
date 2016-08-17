using AtomicChessPuzzles.Configuration;
using AtomicChessPuzzles.Models;
using MongoDB.Driver;

namespace AtomicChessPuzzles.DbRepositories
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
