using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task AddAsync(Attempt attempt)
        {
            await attemptCollection.InsertOneAsync(attempt);
        }

        public async Task<List<Attempt>> GetAsync(int user, int skip, int limit)
        {
            FilterDefinition<Attempt> eqDef = Builders<Attempt>.Filter.Eq("user", user);
            SortDefinition<Attempt> sortDef = Builders<Attempt>.Sort.Descending("endTimestampUtc");
            return await attemptCollection.Find(eqDef).Sort(sortDef).Skip(skip).Limit(limit).ToListAsync();
        }

        public async Task<long> CountAsync(int user)
        {
            FilterDefinition<Attempt> eqDef = Builders<Attempt>.Filter.Eq("user", user);
            return await attemptCollection.CountAsync(eqDef);
        }
    }
}
