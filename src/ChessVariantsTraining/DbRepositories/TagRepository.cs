using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
