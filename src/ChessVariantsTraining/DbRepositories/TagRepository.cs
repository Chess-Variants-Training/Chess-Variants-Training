using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public class TagRepository : ITagRepository
    {
        MongoSettings settings;
        IMongoCollection<PuzzleTag> tagCollection;

        public TagRepository(IOptions<Settings> appSettings)
        {
            settings = appSettings.Value.Mongo;
            GetCollection();
        }

        private void GetCollection()
        {
            MongoClient client = new MongoClient();
            tagCollection = client.GetDatabase(settings.Database).GetCollection<PuzzleTag>(settings.TagCollectionName);
        }

        public async Task<List<PuzzleTag>> TagsByVariantAsync(string variant)
        {
            return await tagCollection.Find(new FilterDefinitionBuilder<PuzzleTag>().Eq("variant", variant)).ToListAsync();
        }

        public async Task<PuzzleTag> FindTag(string variant, string tag)
        {
            return await tagCollection.Find(
                new BsonDocument(
                    new List<BsonElement>
                    {
                        new BsonElement("variant", new BsonString(variant)),
                        new BsonElement("name", new BsonString(tag)) 
                    })
            ).FirstOrDefaultAsync();
        }

        public async Task MaybeAddTagAsync(string variant, string tag)
        {
            FilterDefinitionBuilder<PuzzleTag> builder = new FilterDefinitionBuilder<PuzzleTag>();
            if (await tagCollection.CountAsync(builder.Eq("variant", variant) & builder.Eq("name", tag)) == 0)
            {
                await tagCollection.InsertOneAsync(new PuzzleTag() { Name = tag, Variant = variant });
            }
        }

        public async Task MaybeRemoveTagAsync(string variant, string tag)
        {
            await tagCollection.DeleteOneAsync(new BsonDocument(new List<BsonElement> { new BsonElement("variant", new BsonString(variant)), new BsonElement("name", new BsonString(tag)) }));
        }

        public async Task SetDescription(string variant, string tag, string description)
        {
            UpdateDefinitionBuilder<PuzzleTag> builder = new UpdateDefinitionBuilder<PuzzleTag>();
            UpdateDefinition<PuzzleTag> def = builder.Set("description", description);
            await tagCollection.UpdateOneAsync(
                new BsonDocument(
                    new List<BsonElement>
                    {
                        new BsonElement("variant", new BsonString(variant)),
                        new BsonElement("name", new BsonString(tag)) 
                    }),
                def
            );
        }
    }
}
