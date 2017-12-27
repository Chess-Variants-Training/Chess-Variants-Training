using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using ChessVariantsTraining.Services;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public class PuzzleRepository : IPuzzleRepository
    {
        MongoSettings settings;
        IMongoCollection<Puzzle> puzzleCollection;
        IRandomProvider randomProvider;

        public PuzzleRepository(IOptions<Settings> appSettings, IRandomProvider _randomProvider)
        {
            settings = appSettings.Value.Mongo;
            randomProvider = _randomProvider;
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

        public Puzzle GetOneRandomly(List<int> excludedIds, string variant, int? userId, double nearRating)
        {
            Dictionary<string, object> filterDict = new Dictionary<string, object>()
            {
                ["approved"] = true,
                ["inReview"] = false,
                ["_id"] = new Dictionary<string, object>()
                {
                    ["$nin"] = excludedIds
                }
            };

            if (variant != "Mixed")
            {
                filterDict.Add("variant", new Dictionary<string, object>()
                {
                    ["$eq"] = variant
                });
            }
            if (userId.HasValue)
            {
                filterDict.Add("author", new Dictionary<string, object>()
                {
                    ["$ne"] = userId.Value
                });

                filterDict.Add("reviewers", new Dictionary<string, object>()
                {
                    ["$nin"] = new int[] { userId.Value }
                });
            }

            bool shouldGive1500ProvisionalPuzzle = randomProvider.RandomPositiveInt(6) == 5;
            if (shouldGive1500ProvisionalPuzzle)
            {
                filterDict.Add("rating.value", 1500d);

                Puzzle sel = puzzleCollection.Find(new BsonDocument(filterDict)).FirstOrDefault();
                if (sel != null)
                {
                    return sel;
                }

                filterDict.Remove("rating.value");
            }

            BsonDocument matchDoc = new BsonDocument(new Dictionary<string, object>()
            {
                ["$match"] = filterDict
            });

            BsonDocument sampleDoc = new BsonDocument(new Dictionary<string, object>()
            {
                ["$sample"] = new Dictionary<string, object>() { ["size"] = 30 }
            });

            List<Puzzle> selected = puzzleCollection.Aggregate(
                PipelineDefinition<Puzzle, Puzzle>.Create(
                    matchDoc,
                    sampleDoc
                )
            ).ToList();

            if (!selected.Any())
            {
                return null;
            }

            return selected.Aggregate((x, y) => Math.Abs(x.Rating.Value - nearRating) < Math.Abs(y.Rating.Value - nearRating) ? x : y);
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

        public Puzzle FindByFenAndVariant(string fen, string variant)
        {
            FilterDefinition<Puzzle> filter = Builders<Puzzle>.Filter.Eq("initialfen", fen) & Builders<Puzzle>.Filter.Eq("variant", variant);
            return puzzleCollection.Find(filter).FirstOrDefault();
        }

        public async Task<bool> AddAsync(Puzzle puzzle)
        {
            var found = puzzleCollection.Find(new BsonDocument("_id", new BsonInt32(puzzle.ID)));
            if (found != null && found.Any()) return false;
            try
            {
                await puzzleCollection.InsertOneAsync(puzzle);
            }
            catch (Exception e) when (e is MongoWriteException || e is MongoBulkWriteException)
            {
                return false;
            }
            return true;
        }

        public async Task<Puzzle> GetAsync(int id)
        {
            var found = puzzleCollection.Find(new BsonDocument("_id", new BsonInt32(id)));
            if (found == null) return null;
            return await found.FirstOrDefaultAsync();
        }

        public async Task<Puzzle> GetOneRandomlyAsync(List<int> excludedIds, string variant, int? userId, double nearRating)
        {
            Dictionary<string, object> filterDict = new Dictionary<string, object>()
            {
                ["approved"] = true,
                ["inReview"] = false,
                ["_id"] = new Dictionary<string, object>()
                {
                    ["$nin"] = excludedIds
                }
            };

            if (variant != "Mixed")
            {
                filterDict.Add("variant", new Dictionary<string, object>()
                {
                    ["$eq"] = variant
                });
            }
            if (userId.HasValue)
            {
                filterDict.Add("author", new Dictionary<string, object>()
                {
                    ["$ne"] = userId.Value
                });

                filterDict.Add("reviewers", new Dictionary<string, object>()
                {
                    ["$nin"] = new int[] { userId.Value }
                });
            }

            bool shouldGive1500ProvisionalPuzzle = randomProvider.RandomPositiveInt(6) == 5;
            if (shouldGive1500ProvisionalPuzzle)
            {
                filterDict.Add("rating.value", 1500d);

                Puzzle sel = await puzzleCollection.Find(new BsonDocument(filterDict)).FirstOrDefaultAsync();
                if (sel != null)
                {
                    return sel;
                }

                filterDict.Remove("rating.value");
            }

            BsonDocument matchDoc = new BsonDocument(new Dictionary<string, object>()
            {
                ["$match"] = filterDict
            });

            BsonDocument sampleDoc = new BsonDocument(new Dictionary<string, object>()
            {
                ["$sample"] = new Dictionary<string, object>() { ["size"] = 30 }
            });

            List<Puzzle> selected = await (await puzzleCollection.AggregateAsync(
                PipelineDefinition<Puzzle, Puzzle>.Create(
                    matchDoc,
                    sampleDoc
                )
            )).ToListAsync();

            if (!selected.Any())
            {
                return null;
            }

            return selected.Aggregate((x, y) => Math.Abs(x.Rating.Value - nearRating) < Math.Abs(y.Rating.Value - nearRating) ? x : y);
        }

        public async Task<DeleteResult> RemoveAsync(int id)
        {
            return await puzzleCollection.DeleteOneAsync(new BsonDocument("_id", new BsonInt32(id)));
        }

        public async Task<DeleteResult> RemoveAllByAsync(int author)
        {
            return await puzzleCollection.DeleteManyAsync(new BsonDocument("author", new BsonInt32(author)));
        }

        public async Task<bool> UpdateRatingAsync(int id, Rating newRating)
        {
            UpdateDefinitionBuilder<Puzzle> builder = new UpdateDefinitionBuilder<Puzzle>();
            UpdateDefinition<Puzzle> def = builder.Set("rating", newRating);
            UpdateResult result = await puzzleCollection.UpdateOneAsync(new BsonDocument("_id", new BsonInt32(id)), def);
            return result.IsAcknowledged && result.MatchedCount != 0;
        }

        public async Task<List<Puzzle>> InReviewAsync()
        {
            FilterDefinition<Puzzle> filter = Builders<Puzzle>.Filter.Eq("inReview", true);
            return await puzzleCollection.Find(filter).ToListAsync();
        }

        public async Task<bool> ApproveAsync(int id, int reviewer)
        {
            UpdateDefinitionBuilder<Puzzle> builder = Builders<Puzzle>.Update;
            UpdateDefinition<Puzzle> def = builder.Set("approved", true).Set("inReview", false).Push("reviewers", reviewer);
            UpdateResult result = await puzzleCollection.UpdateOneAsync(new BsonDocument("_id", new BsonInt32(id)), def);
            return result.IsAcknowledged && result.MatchedCount != 0;
        }

        public async Task<bool> RejectAsync(int id, int reviewer)
        {
            UpdateDefinitionBuilder<Puzzle> builder = Builders<Puzzle>.Update;
            UpdateDefinition<Puzzle> def = builder.Set("approved", false).Set("inReview", false).Push("reviewers", reviewer);
            UpdateResult result = await puzzleCollection.UpdateOneAsync(new BsonDocument("_id", new BsonInt32(id)), def);
            return result.IsAcknowledged && result.MatchedCount != 0;
        }

        public async Task<Puzzle> FindByFenAndVariantAsync(string fen, string variant)
        {
            FilterDefinition<Puzzle> filter = Builders<Puzzle>.Filter.Eq("initialfen", fen) & Builders<Puzzle>.Filter.Eq("variant", variant);
            return await puzzleCollection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
