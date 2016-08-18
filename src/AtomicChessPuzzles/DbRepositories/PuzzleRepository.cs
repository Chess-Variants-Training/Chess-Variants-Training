using AtomicChessPuzzles.Configuration;
using AtomicChessPuzzles.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtomicChessPuzzles.DbRepositories
{
    public class PuzzleRepository : IPuzzleRepository
    {
        MongoSettings settings;
        IMongoCollection<Puzzle> puzzleCollection;
        Random rnd = new Random();

        public PuzzleRepository(ISettings appSettings)
        {
            settings = appSettings.Mongo;
            GetCollection();
        }

        private void GetCollection()
        {
            MongoClient client = new MongoClient();
            puzzleCollection = client.GetDatabase(settings.Database).GetCollection<Puzzle>(settings.PuzzleCollectionName);
        }

        public bool Add(Puzzle puzzle)
        {
            var found = puzzleCollection.Find(new BsonDocument("_id", new BsonString(puzzle.ID)));
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

        public Puzzle Get(string id)
        {
            var found = puzzleCollection.Find(new BsonDocument("_id", new BsonString(id)));
            if (found == null) return null;
            return found.FirstOrDefault();
        }

        public Puzzle GetOneRandomly(List<string> excludedIds, double nearRating = 1500)
        {
            FilterDefinitionBuilder<Puzzle> filterBuilder = Builders<Puzzle>.Filter;
            FilterDefinition<Puzzle> filter = filterBuilder.Nin("_id", excludedIds) & filterBuilder.Eq("inReview", false)
                                              & filterBuilder.Eq("approved", true);
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

        public DeleteResult Remove(string id)
        {
            return puzzleCollection.DeleteOne(new BsonDocument("_id", new BsonString(id)));
        }

        public DeleteResult RemoveAllBy(int author)
        {
            return puzzleCollection.DeleteMany(new BsonDocument("author", new BsonInt32(author)));
        }

        public bool UpdateRating(string id, Rating newRating)
        {
            UpdateDefinitionBuilder<Puzzle> builder = new UpdateDefinitionBuilder<Puzzle>();
            UpdateDefinition<Puzzle> def = builder.Set("rating", newRating);
            UpdateResult result = puzzleCollection.UpdateOne(new BsonDocument("_id", new BsonString(id)), def);
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

        public bool Approve(string id)
        {
            UpdateDefinitionBuilder<Puzzle> builder = Builders<Puzzle>.Update;
            UpdateDefinition<Puzzle> def = builder.Set("approved", true).Set("inReview", false);
            UpdateResult result = puzzleCollection.UpdateOne(new BsonDocument("_id", new BsonString(id)), def);
            return result.IsAcknowledged && result.MatchedCount != 0;
        }

        public bool Reject(string id)
        {
            UpdateDefinitionBuilder<Puzzle> builder = Builders<Puzzle>.Update;
            UpdateDefinition<Puzzle> def = builder.Set("approved", false).Set("inReview", false);
            UpdateResult result = puzzleCollection.UpdateOne(new BsonDocument("_id", new BsonString(id)), def);
            return result.IsAcknowledged && result.MatchedCount != 0;
        }
    }
}
