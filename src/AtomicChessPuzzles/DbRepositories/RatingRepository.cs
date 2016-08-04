using AtomicChessPuzzles.Configuration;
using AtomicChessPuzzles.Models;
using MongoDB.Driver;

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
    }
}
