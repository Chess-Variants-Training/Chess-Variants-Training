using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ChessVariantsTraining.DbRepositories
{
    public class ReportRepository : IReportRepository
    {
        MongoSettings settings;
        IMongoCollection<Report> reportCollection;

        public ReportRepository(ISettings appSettings)
        {
            settings = appSettings.Mongo;
            GetCollection();
        }

        private void GetCollection()
        {
            MongoClient client = new MongoClient();
            reportCollection = client.GetDatabase(settings.Database).GetCollection<Report>(settings.ReportCollectionName);
        }

        public bool Add(Report report)
        {
            var found = reportCollection.Find(new BsonDocument("_id", new BsonString(report.ID)));
            if (found != null && found.Any()) return false;
            try
            {
                reportCollection.InsertOne(report);
            }
            catch (Exception e) when (e is MongoWriteException || e is MongoBulkWriteException)
            {
                return false;
            }
            return true;
        }

        private bool Handle(string reportId, string judgement)
        {
            UpdateDefinition<Report> updateDef = Builders<Report>.Update.Set("handled", true).Set("judgementAfterHandling", judgement);
            FilterDefinition<Report> filter = Builders<Report>.Filter.Eq("id", reportId);
            UpdateResult updateResult = reportCollection.UpdateOne(filter, updateDef);
            return updateResult.IsAcknowledged && updateResult.MatchedCount != 0;
        }

        public bool MarkHelpful(string reportId)
        {
            return Handle(reportId, "helpful");
        }

        public bool MarkDeclined(string reportId)
        {
            return Handle(reportId, "declined");
        }

        public List<Report> GetByType(string type)
        {
            FilterDefinition<Report> filter = Builders<Report>.Filter.Eq("type", type);
            var found = reportCollection.Find(filter);
            if (found == null)
            {
                return new List<Report>();
            }
            return found.ToList();
        }

        public List<Report> GetByTypes(IEnumerable<string> types)
        {
            FilterDefinition<Report> filter = Builders<Report>.Filter.In("type", types);
            return reportCollection.Find(filter).ToList();
        }
    }
}
