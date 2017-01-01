using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models.Variant960;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories.Variant960
{
    public class GameRepository : IGameRepository
    {
        MongoSettings settings;
        IMongoCollection<Game> gameCollection;

        public GameRepository(IOptions<Settings> appSettings)
        {
            settings = appSettings.Value.Mongo;
            GetCollection();
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
    }
}
