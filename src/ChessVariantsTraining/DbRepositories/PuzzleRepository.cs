using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessVariantsTraining.DbRepositories
{
    public class PuzzleRepository : IPuzzleRepository
    {
        MongoSettings settings;
        IMongoCollection<Puzzle> puzzleCollection;
        Random rnd = new Random();

        public PuzzleRepository(IOptions<Settings> appSettings)
        {
            settings = appSettings.Value.Mongo;
            GetCollection();
        }

        private void GetCollection()
        {
            MongoClient client = new MongoClient();
            puzzleCollection = client.GetDatabase(settings.Database).GetCollection<Puzzle>(settings.PuzzleCollectionName);
        }

        public bool Add(Puzzle puzzle)
        {
            var found = puzzleCollection.Find(new BsonDocument("_id", new BsonInt32(puzzle.ID)));
            if (found != null && found.Any()) return false;
            try
            {
                puzzleCollection.InsertOne(puzzle);
            }
            catch (Exception e) when (e is MongoWriteException || e is MongoBulkWriteException)
            {
                return false;
            }
            return true;
        }

        public Puzzle Get(int id)
        {
            var found = puzzleCollection.Find(new BsonDocument("_id", new BsonInt32(id)));
            if (found == null) return null;
            return found.FirstOrDefault();
        }

        public Puzzle GetOneRandomly(List<int> excludedIds, string variant, int? userId, double nearRating = 1500)
        {
            FilterDefinitionBuilder<Puzzle> filterBuilder = Builders<Puzzle>.Filter;
            FilterDefinition<Puzzle> filter = filterBuilder.Nin("_id", excludedIds) & filterBuilder.Eq("inReview", false)
                                              & filterBuilder.Eq("approved", true);
            if (variant != "Mixed")
            {
                filter &= filterBuilder.Eq("variant", variant);
            }
            if (userId.HasValue)
            {
                filter &= filterBuilder.Ne("author", userId.Value) & filterBuilder.Nin("reviewers", new int[] { userId.Value });
            }
            FilterDefinition<Puzzle> lteFilter = filter;
            FilterDefinition<Puzzle> gtFilter = filter;
            bool higherRated = RandomBoolean();
            gtFilter &= filterBuilder.Gt("rating.value", nearRating);
            lteFilter &= filterBuilder.Lte("rating.value", nearRating);
            var foundGt = puzzleCollection.Find(gtFilter);
            var foundLte = puzzleCollection.Find(lteFilter);
            if (foundGt == null && foundLte == null) return null;
            SortDefinitionBuilder<Puzzle> sortBuilder = Builders<Puzzle>.Sort;
            foundGt = foundGt.Sort(sortBuilder.Ascending("rating.value")).Limit(1);
            foundLte = foundLte.Sort(sortBuilder.Descending("rating.value")).Limit(1);
            Puzzle oneGt = foundGt.FirstOrDefault();
            Puzzle oneLte = foundLte.FirstOrDefault();
            if (oneGt == null) return oneLte;
            else if (oneLte == null) return oneGt;
            else return RandomBoolean() ? oneGt : oneLte;
        }

        public DeleteResult Remove(int id)
        {
            return puzzleCollection.DeleteOne(new BsonDocument("_id", new BsonInt32(id)));
        }

        public DeleteResult RemoveAllBy(int author)
        {
            return puzzleCollection.DeleteMany(new BsonDocument("author", new BsonInt32(author)));
        }

        public bool UpdateRating(int id, Rating newRating)
        {
            UpdateDefinitionBuilder<Puzzle> builder = new UpdateDefinitionBuilder<Puzzle>();
            UpdateDefinition<Puzzle> def = builder.Set("rating", newRating);
            UpdateResult result = puzzleCollection.UpdateOne(new BsonDocument("_id", new BsonInt32(id)), def);
            return result.IsAcknowledged && result.MatchedCount != 0;
        }

        private bool RandomBoolean()
        {
            return rnd.Next() % 2 == 0;
        }

        public List<Puzzle> InReview()
        {
            FilterDefinition<Puzzle> filter = Builders<Puzzle>.Filter.Eq("inReview", true);
            return puzzleCollection.Find(filter).ToList();
        }

        public bool Approve(int id, int reviewer)
        {
            UpdateDefinitionBuilder<Puzzle> builder = Builders<Puzzle>.Update;
            UpdateDefinition<Puzzle> def = builder.Set("approved", true).Set("inReview", false).Push("reviewers", reviewer);
            UpdateResult result = puzzleCollection.UpdateOne(new BsonDocument("_id", new BsonInt32(id)), def);
            return result.IsAcknowledged && result.MatchedCount != 0;
        }

        public bool Reject(int id, int reviewer)
        {
            UpdateDefinitionBuilder<Puzzle> builder = Builders<Puzzle>.Update;
            UpdateDefinition<Puzzle> def = builder.Set("approved", false).Set("inReview", false).Push("reviewers", reviewer);
            UpdateResult result = puzzleCollection.UpdateOne(new BsonDocument("_id", new BsonInt32(id)), def);
            return result.IsAcknowledged && result.MatchedCount != 0;
        }
    }
}
