using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Linq;

namespace ChessVariantsTraining.DbRepositories
{
    public class PositionRepository : IPositionRepository
    {
        MongoSettings settings;
        IMongoCollection<TrainingPosition> positionCollection;
        Random rnd = new Random();

        public PositionRepository(IOptions<Settings> appSettings)
        {
            settings = appSettings.Value.Mongo;
            GetCollection();
        }

        private void GetCollection()
        {
            MongoClient client = new MongoClient();
            positionCollection = client.GetDatabase(settings.Database).GetCollection<TrainingPosition>(settings.PositionCollectionName);
        }

        public TrainingPosition GetRandomMateInOne()
        {
            double x = rnd.NextDouble();
            double y = rnd.NextDouble();
            FilterDefinitionBuilder<TrainingPosition> filterBuilder = Builders<TrainingPosition>.Filter;
            FilterDefinition<TrainingPosition> filter = filterBuilder.Eq("type", "mateInOne") & filterBuilder.Near("location", x, y);
            var found = positionCollection.Find(filter);
            if (found == null) return null;
            else return found.Limit(1).First();
        }
    }
}