using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using MongoDB.Driver;
using System.Linq;

namespace ChessVariantsTraining.DbRepositories
{
    public class SavedLoginRepository : ISavedLoginRepository
    {
        MongoSettings settings;
        IMongoCollection<SavedLogin> savedLoginCollection;

        public SavedLoginRepository(ISettings appSettings)
        {
            settings = appSettings.Mongo;
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
    }
}
