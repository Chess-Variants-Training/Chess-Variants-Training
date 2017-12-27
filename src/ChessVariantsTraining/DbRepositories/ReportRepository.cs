using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public class ReportRepository : IReportRepository
    {
        MongoSettings settings;
        IMongoCollection<Report> reportCollection;

        public ReportRepository(IOptions<Settings> appSettings)
        {
            settings = appSettings.Value.Mongo;
            GetCollection();
        }

        private void GetCollection()
        {
            MongoClient client = new MongoClient();
            reportCollection = client.GetDatabase(settings.Database).GetCollection<Report>(settings.ReportCollectionName);
        }

        public async Task<bool> AddAsync(Report report)
        {
            var found = reportCollection.Find(new BsonDocument("_id", new BsonString(report.ID)));
            if (found != null && await found.AnyAsync()) return false;
            try
            {
                await reportCollection.InsertOneAsync(report);
            }
            catch (Exception e) when (e is MongoWriteException || e is MongoBulkWriteException)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> HandleAsync(string reportId, string judgement)
        {
            UpdateDefinition<Report> updateDef = Builders<Report>.Update.Set("handled", true).Set("judgementAfterHandling", judgement);
            FilterDefinition<Report> filter = Builders<Report>.Filter.Eq("_id", reportId);
            UpdateResult updateResult = await reportCollection.UpdateOneAsync(filter, updateDef);
            return updateResult.IsAcknowledged && updateResult.MatchedCount != 0;
        }

        public async Task<List<Report>> GetUnhandledByTypeAsync(string type)
        {
            FilterDefinition<Report> filter = Builders<Report>.Filter.Eq("type", type) & Builders<Report>.Filter.Eq("handled", false);
            var found = reportCollection.Find(filter);
            if (found == null)
            {
                return new List<Report>();
            }
            return await found.ToListAsync();
        }

        public async Task<List<Report>> GetUnhandledByTypesAsync(IEnumerable<string> types)
        {
            FilterDefinition<Report> filter = Builders<Report>.Filter.In("type", types) & Builders<Report>.Filter.Eq("handled", false);
            return await reportCollection.Find(filter).ToListAsync();
        }

        public async Task<Report> GetByIdAsync(string id)
        {
            return await reportCollection.Find(Builders<Report>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
        }
    }
}
