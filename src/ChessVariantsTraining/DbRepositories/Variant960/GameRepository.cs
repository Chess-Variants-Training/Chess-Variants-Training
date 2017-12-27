using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models.Variant960;
using ChessVariantsTraining.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories.Variant960
{
    public class GameRepository : IGameRepository
    {
        MongoSettings settings;
        IMongoCollection<Game> gameCollection;
        IRandomProvider randomProvider;

        public GameRepository(IOptions<Settings> appSettings, IRandomProvider _randomProvider)
        {
            settings = appSettings.Value.Mongo;
            GetCollection();
            randomProvider = _randomProvider;
        }

        void GetCollection()
        {
            MongoClient client = new MongoClient();
            gameCollection = client.GetDatabase(settings.Database).GetCollection<Game>(settings.GameCollectionName);
        }

        public Game Get(string id)
        {
            return gameCollection.Find(Builders<Game>.Filter.Eq("_id", id)).FirstOrDefault();
        }

        public async Task AddAsync(Game game)
        {
            await gameCollection.InsertOneAsync(game);
        }

        public async Task<Game> GetAsync(string id)
        {
            return await gameCollection.Find(Builders<Game>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Game game)
        {
            await gameCollection.ReplaceOneAsync(Builders<Game>.Filter.Eq("_id", game.ID), game);
        }

        public async Task<string> GenerateIdAsync()
        {
            bool okay;
            string generated;
            do
            {
                generated = randomProvider.RandomString(8).ToLowerInvariant();
                okay = await GetAsync(generated) == null;
            } while (!okay);
            return generated;
        }

        public async Task<List<Game>> GetByPlayerIdAsync(int id, int skip, int limit)
        {
            FilterDefinitionBuilder<Game> filterBuilder = Builders<Game>.Filter;
            FilterDefinition<Game> whiteEq = filterBuilder.Eq("white.userId", id);
            FilterDefinition<Game> blackEq = filterBuilder.Eq("black.userId", id);
            SortDefinition<Game> sortDef = Builders<Game>.Sort.Descending("startedUtc");
            return await gameCollection.Find(filterBuilder.Or(whiteEq, blackEq)).Sort(sortDef).Skip(skip).Limit(limit).ToListAsync();
        }

        public async Task<long> CountByPlayerIdAsync(int id)
        {
            FilterDefinitionBuilder<Game> filterBuilder = Builders<Game>.Filter;
            FilterDefinition<Game> whiteEq = filterBuilder.Eq("white.userId", id);
            FilterDefinition<Game> blackEq = filterBuilder.Eq("black.userId", id);
            return await gameCollection.CountAsync(filterBuilder.Or(whiteEq, blackEq));
        }
    }
}
