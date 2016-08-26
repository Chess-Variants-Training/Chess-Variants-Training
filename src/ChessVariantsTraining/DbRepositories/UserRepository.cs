using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;

namespace ChessVariantsTraining.DbRepositories
{
    public class UserRepository : IUserRepository
    {
        MongoSettings settings;
        IMongoCollection<User> userCollection;

        public UserRepository(IOptions<Settings> appSettings)
        {
            settings = appSettings.Value.Mongo;
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

        public User FindById(int id)
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Eq("_id", id);
            return userCollection.Find(filter).FirstOrDefault();
        }

        public User FindByUsername(string name)
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Text(name, new TextSearchOptions() { CaseSensitive = false });
            return userCollection.Find(filter).FirstOrDefault();
        }
    }
}
