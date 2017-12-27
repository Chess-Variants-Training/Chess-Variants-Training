using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{

    public class CommentRepository : ICommentRepository
    {
        MongoSettings settings;
        IMongoCollection<Comment> commentCollection;

        public CommentRepository(IOptions<Settings> appSettings)
        {
            settings = appSettings.Value.Mongo;
            GetCollection();
        }

        private void GetCollection()
        {
            MongoClient client = new MongoClient();
            commentCollection = client.GetDatabase(settings.Database).GetCollection<Comment>(settings.CommentCollectionName);
        }

        public bool Add(Comment comment)
        {
            var found = commentCollection.Find(new BsonDocument("_id", new BsonInt32(comment.ID)));
            if (found != null && found.Any()) return false;
            try
            {
                commentCollection.InsertOne(comment);
            }
            catch (Exception e) when (e is MongoWriteException || e is MongoBulkWriteException)
            {
                return false;
            }
            return true;
        }

        public Comment GetById(int id)
        {
            var found = commentCollection.Find(new BsonDocument("_id", new BsonInt32(id)));
            if (found == null) return null;
            return found.FirstOrDefault();
        }

        public List<Comment> GetByPuzzle(int puzzleId)
        {
            return commentCollection.Find(new BsonDocument("puzzleId", new BsonInt32(puzzleId))).ToList();
        }

        public bool Edit(int id, string newBodyUnsanitized)
        {
            FilterDefinition<Comment> filter = Builders<Comment>.Filter.Eq("_id", id);
            UpdateDefinition<Comment> update = Builders<Comment>.Update.Set("bodyUnsanitized", newBodyUnsanitized);
            UpdateResult res = commentCollection.UpdateOne(filter, update);
            return res.IsAcknowledged && res.MatchedCount != 0;
        }

        public bool SoftDelete(int id)
        {
            FilterDefinition<Comment> filter = Builders<Comment>.Filter.Eq("_id", id);
            UpdateDefinition<Comment> update = Builders<Comment>.Update.Set("deleted", true);
            UpdateResult res = commentCollection.UpdateOne(filter, update);
            return res.IsAcknowledged && res.MatchedCount != 0;
        }

        public async Task<bool> AddAsync(Comment comment)
        {
            var found = commentCollection.Find(new BsonDocument("_id", new BsonInt32(comment.ID)));
            if (found != null && found.Any()) return false;
            try
            {
                await commentCollection.InsertOneAsync(comment);
            }
            catch (Exception e) when (e is MongoWriteException || e is MongoBulkWriteException)
            {
                return false;
            }
            return true;
        }

        public async Task<Comment> GetByIdAsync(int id)
        {
            var found = commentCollection.Find(new BsonDocument("_id", new BsonInt32(id)));
            return await found.FirstOrDefaultAsync();
        }

        public async Task<List<Comment>> GetByPuzzleAsync(int puzzleId)
        {
            return await commentCollection.Find(new BsonDocument("puzzleId", new BsonInt32(puzzleId))).ToListAsync();
        }

        public async Task<bool> EditAsync(int id, string newBodyUnsanitized)
        {
            FilterDefinition<Comment> filter = Builders<Comment>.Filter.Eq("_id", id);
            UpdateDefinition<Comment> update = Builders<Comment>.Update.Set("bodyUnsanitized", newBodyUnsanitized);
            UpdateResult res = await commentCollection.UpdateOneAsync(filter, update);
            return res.IsAcknowledged && res.MatchedCount != 0;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            FilterDefinition<Comment> filter = Builders<Comment>.Filter.Eq("_id", id);
            UpdateDefinition<Comment> update = Builders<Comment>.Update.Set("deleted", true);
            UpdateResult res = await commentCollection.UpdateOneAsync(filter, update);
            return res.IsAcknowledged && res.MatchedCount != 0;
        }
    }
}
