using AtomicChessPuzzles.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;

namespace AtomicChessPuzzles.DbRepositories
{
    public class PositionRepository : IPositionRepository
    {
        MongoSettings settings;
        IMongoCollection<TrainingPosition> positionCollection;

        public PositionRepository()
        {
            settings = new MongoSettings();
            GetCollection();
        }

        private void GetCollection()
        {
            MongoClient client = new MongoClient();
            positionCollection = client.GetDatabase(settings.Database).GetCollection<TrainingPosition>(settings.PositionCollectionName);
        }

        public TrainingPosition GetRandomMateInOne()
        {
            var found = positionCollection.Find(new BsonDocument("type", new BsonString("mateInOne")));
            if (found == null) return null;
            else return found.First(); // TODO: actual randomness
        }
    }
}