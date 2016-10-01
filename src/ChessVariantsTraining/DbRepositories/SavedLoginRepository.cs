using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq;

namespace ChessVariantsTraining.DbRepositories
{
    public class SavedLoginRepository : ISavedLoginRepository
    {
        MongoSettings settings;
        IMongoCollection<SavedLogin> savedLoginCollection;

        public SavedLoginRepository(IOptions<Settings> appSettings)
        {
            settings = appSettings.Value.Mongo;
            GetCollection();
        }

        void GetCollection()
        {
            MongoClient client = new MongoClient(settings.MongoConnectionString);
            savedLoginCollection = client.GetDatabase(settings.Database).GetCollection<SavedLogin>(settings.SavedLoginCollectionName);
        }

        public void Add(SavedLogin login)
        {
            savedLoginCollection.InsertOne(login);
        }

        public bool ContainsID(long id)
        {
            return savedLoginCollection.Find(Builders<SavedLogin>.Filter.Eq("_id", id)).Any();
        }

        public int? AuthenticatedUser(long loginId, byte[] hashedToken)
        {
            FilterDefinitionBuilder<SavedLogin> builder = Builders<SavedLogin>.Filter;
            FilterDefinition<SavedLogin> filter = builder.Eq("_id", loginId) & builder.Eq("hashedToken", hashedToken);
            SavedLogin found = savedLoginCollection.Find(filter).FirstOrDefault();
            return found?.User;
        }

        public void Delete(long id)
        {
            FilterDefinition<SavedLogin> filter = Builders<SavedLogin>.Filter.Eq("_id", id);
            savedLoginCollection.DeleteOne(filter);
        }

        public void DeleteAllOfExcept(int userId, long excludedId)
        {
            FilterDefinition<SavedLogin> filter = Builders<SavedLogin>.Filter.Ne("_id", excludedId) & Builders<SavedLogin>.Filter.Eq("user", userId);
            savedLoginCollection.DeleteMany(filter);
        }
    }
}
