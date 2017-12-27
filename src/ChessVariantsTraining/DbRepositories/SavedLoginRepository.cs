using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;

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

        public void DeleteAllOf(int userId)
        {
            FilterDefinition<SavedLogin> filter = Builders<SavedLogin>.Filter.Eq("user", userId);
            savedLoginCollection.DeleteMany(filter);
        }

        public async Task AddAsync(SavedLogin login)
        {
            await savedLoginCollection.InsertOneAsync(login);
        }

        public async Task<bool> ContainsIDAsync(long id)
        {
            return await savedLoginCollection.Find(Builders<SavedLogin>.Filter.Eq("_id", id)).AnyAsync();
        }

        public async Task<int?> AuthenticatedUserAsync(long loginId, byte[] hashedToken)
        {
            FilterDefinitionBuilder<SavedLogin> builder = Builders<SavedLogin>.Filter;
            FilterDefinition<SavedLogin> filter = builder.Eq("_id", loginId) & builder.Eq("hashedToken", hashedToken);
            SavedLogin found = await savedLoginCollection.Find(filter).FirstOrDefaultAsync();
            return found?.User;
        }

        public async Task DeleteAsync(long id)
        {
            FilterDefinition<SavedLogin> filter = Builders<SavedLogin>.Filter.Eq("_id", id);
            await savedLoginCollection.DeleteOneAsync(filter);
        }

        public async Task DeleteAllOfExceptAsync(int userId, long excludedId)
        {
            FilterDefinition<SavedLogin> filter = Builders<SavedLogin>.Filter.Ne("_id", excludedId) & Builders<SavedLogin>.Filter.Eq("user", userId);
            await savedLoginCollection.DeleteManyAsync(filter);
        }

        public async Task DeleteAllOfAsync(int userId)
        {
            FilterDefinition<SavedLogin> filter = Builders<SavedLogin>.Filter.Eq("user", userId);
            await savedLoginCollection.DeleteManyAsync(filter);
        }
    }
}
