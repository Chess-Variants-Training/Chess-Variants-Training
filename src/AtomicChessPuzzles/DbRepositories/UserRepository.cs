using MongoDB.Driver;
using AtomicChessPuzzles.Models;
using MongoDB.Bson;

namespace AtomicChessPuzzles.DbRepositories
{
    public class UserRepository : IUserRepository
    {
        MongoSettings settings;
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

        public bool Add(User user)
        {
            bool exists = userCollection.FindSync<User>(new ExpressionFilterDefinition<User>(x => x.Username == user.Username)).Any();
            if (exists) return false;
            userCollection.InsertOne(user);
            return true;
        }

        public void Update(User user)
        {
            userCollection.ReplaceOne(new ExpressionFilterDefinition<User>(x => x.Username == user.Username), user);
        }

        public void Delete(User user)
        {
            userCollection.DeleteOne(new ExpressionFilterDefinition<User>(x => x.Username == user.Username));
        }
    }
}
