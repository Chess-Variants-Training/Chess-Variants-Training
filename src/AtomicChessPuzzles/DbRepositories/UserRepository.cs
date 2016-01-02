using MongoDB.Driver;
using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.DbRepositories
{
    public class UserRepository : IUserRepository
    {
        MongoSettings settings;
        IMongoDatabase database;
        IMongoCollection<User> userCollection;

        public UserRepository()
        {
            settings = new MongoSettings();
        }

        private void GetDatabase()
        {
            MongoClient client = new MongoClient(settings.MongoConnectionString);
            userCollection = client.GetDatabase(settings.Database).GetCollection<User>(settings.UserCollectionName);
        }

        public void Add(User user)
        {
            userCollection.InsertOne(user);
        }

        public void Update(User user)
        {
            userCollection.ReplaceOne(new ExpressionFilterDefinition<User>(x => x.ID == user.ID), user);
        }

        public void Delete(User user)
        {
            userCollection.DeleteOne(new ExpressionFilterDefinition<User>(x => x.ID == user.ID));
        }
    }
}
