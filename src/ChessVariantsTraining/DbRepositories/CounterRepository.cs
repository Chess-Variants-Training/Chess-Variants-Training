using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public class CounterRepository : ICounterRepository
    {
        MongoSettings settings;
        IMongoCollection<Counter> counterCollection;

        public CounterRepository(IOptions<Settings> appSettings)
        {
            settings = appSettings.Value.Mongo;
            GetCollection();
        }

        void GetCollection()
        {
            MongoClient client = new MongoClient(settings.MongoConnectionString);
            counterCollection = client.GetDatabase(settings.Database).GetCollection<Counter>(settings.CounterCollectionName);
        }

        public int GetAndIncrease(string id)
        {
            FilterDefinition<Counter> filter = Builders<Counter>.Filter.Eq("_id", id);
            UpdateDefinition<Counter> update = Builders<Counter>.Update.Inc("next", 1);
            return counterCollection.FindOneAndUpdate(filter, update).Next;
        }

        public async Task<int> GetAndIncreaseAsync(string id)
        {
            FilterDefinition<Counter> filter = Builders<Counter>.Filter.Eq("_id", id);
            UpdateDefinition<Counter> update = Builders<Counter>.Update.Inc("next", 1);
            return (await counterCollection.FindOneAndUpdateAsync(filter, update)).Next;
        }
    }
}
