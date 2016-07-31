using AtomicChessPuzzles.Configuration;
using AtomicChessPuzzles.Models;
using MongoDB.Driver;
using System;

namespace AtomicChessPuzzles.DbRepositories
{
    public class UserRepository : IUserRepository
    {
        MongoSettings settings;
        IMongoCollection<User> userCollection;

        public UserRepository(ISettings appSettings)
        {
            settings = appSettings.Mongo;
            GetCollection();
        }

        private void GetCollection()
        {
            MongoClient client = new MongoClient(settings.MongoConnectionString);
            userCollection = client.GetDatabase(settings.Database).GetCollection<User>(settings.UserCollectionName);
        }

        public bool Add(User user)
        {
            var found = userCollection.FindSync<User>(new ExpressionFilterDefinition<User>(x => x.ID == user.ID));
            if (found != null && found.Any()) return false;
            try
            {
                userCollection.InsertOne(user);
            }
            catch (Exception e) when (e is MongoWriteException || e is MongoBulkWriteException)
            {
                return false;
            }
            return true;
        }

        public void Update(User user)
        {
            userCollection.ReplaceOne(new ExpressionFilterDefinition<User>(x => x.ID == user.ID), user);
        }

        public void Delete(User user)
        {
            userCollection.DeleteOne(new ExpressionFilterDefinition<User>(x => x.ID == user.ID));
        }

        public User FindByUsername(string name)
        {
            string id = name.ToLowerInvariant();
            var found = userCollection.FindSync<User>(new ExpressionFilterDefinition<User>(x => x.ID == id));
            if (found == null) return null;
            return found.FirstOrDefault();
        }
    }
}
