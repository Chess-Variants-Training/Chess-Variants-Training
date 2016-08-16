using AtomicChessPuzzles.Configuration;
using AtomicChessPuzzles.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace AtomicChessPuzzles.DbRepositories
{
    public class RatingRepository : IRatingRepository
    {
        MongoSettings settings;
        IMongoCollection<RatingWithMetadata> ratingCollection;

        public RatingRepository(ISettings appSettings)
        {
            settings = appSettings.Mongo;
            GetCollection();
        }

        void GetCollection()
        {
            MongoClient client = new MongoClient();
            ratingCollection = client.GetDatabase(settings.Database).GetCollection<RatingWithMetadata>(settings.RatingCollectionName);
        }

        public void Add(RatingWithMetadata ratingWithMetadata)
        {
            ratingCollection.InsertOne(ratingWithMetadata);
        }

        public List<RatingWithMetadata> GetFor(string user)
        {
            FilterDefinition<RatingWithMetadata> filter = Builders<RatingWithMetadata>.Filter.Eq("owner", user.ToLower());
            return ratingCollection.Find(filter).ToList();
        }

        public List<RatingWithMetadata> GetForUserOnRange(string user, DateTime from, DateTime to)
        {
            FilterDefinitionBuilder<RatingWithMetadata> builder = Builders<RatingWithMetadata>.Filter;
            FilterDefinition<RatingWithMetadata> filter = builder.Eq("owner", user.ToLower()) & builder.Lte("timestampUtc", to) & builder.Gte("timestampUtc", from);
            return ratingCollection.Find(filter).ToList();
        }
    }
}
