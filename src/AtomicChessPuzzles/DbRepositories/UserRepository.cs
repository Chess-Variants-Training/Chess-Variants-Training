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
            GetCollection();
        }

        private void GetCollection()
        {
            MongoClient client = new MongoClient(settings.MongoConnectionString);
            userCollection = client.GetDatabase(settings.Database).GetCollection<User>(settings.UserCollectionName);
        }

        public bool Add(User user)
        {
            var found = userCollection.FindSync<User>(new ExpressionFilterDefinition<User>(x => x.Username == user.Username));
            if (found != null && found.Any()) return false;
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

        public User FindByUsername(string name)
        {
            var found = userCollection.FindSync<User>(new ExpressionFilterDefinition<User>(x => x.Username == name));
            if (found == null) return null;
            return found.FirstOrDefault();
        }
    }
}
