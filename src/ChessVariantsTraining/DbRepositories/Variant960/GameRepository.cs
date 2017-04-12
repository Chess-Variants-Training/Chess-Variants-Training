using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models.Variant960;
using ChessVariantsTraining.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

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

        public void Add(Game game)
        {
            gameCollection.InsertOne(game);
        }

        public Game Get(string id)
        {
            return gameCollection.Find(Builders<Game>.Filter.Eq("_id", id)).FirstOrDefault();
        }

        public void Update(Game game)
        {
            gameCollection.ReplaceOne(Builders<Game>.Filter.Eq("_id", game.ID), game);
        }

        public string GenerateId()
        {
            bool okay;
            string generated;
            do
            {
                generated = randomProvider.RandomString(8).ToLowerInvariant();
                okay = Get(generated) == null;
            } while (!okay);
            return generated;
        }

        public List<Game> GetByPlayerId(int id, int skip, int limit)
        {
            FilterDefinitionBuilder<Game> filterBuilder = Builders<Game>.Filter;
            FilterDefinition<Game> whiteEq = filterBuilder.Eq("white.userId", id);
            FilterDefinition<Game> blackEq = filterBuilder.Eq("black.userId", id);
            SortDefinition<Game> sortDef = Builders<Game>.Sort.Descending("startedUtc");
            return gameCollection.Find(filterBuilder.Or(whiteEq, blackEq)).Sort(sortDef).Skip(skip).Limit(limit).ToList();
        }

        public long CountByPlayerId(int id)
        {
            FilterDefinitionBuilder<Game> filterBuilder = Builders<Game>.Filter;
            FilterDefinition<Game> whiteEq = filterBuilder.Eq("white.userId", id);
            FilterDefinition<Game> blackEq = filterBuilder.Eq("black.userId", id);
            return gameCollection.Count(filterBuilder.Or(whiteEq, blackEq));
        }
    }
}
