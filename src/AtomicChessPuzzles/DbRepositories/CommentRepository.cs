using AtomicChessPuzzles.Configuration;
using AtomicChessPuzzles.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace AtomicChessPuzzles.DbRepositories
{

    public class CommentRepository : ICommentRepository
    {
        MongoSettings settings;
        IMongoCollection<Comment> commentCollection;

        public CommentRepository(ISettings appSettings)
        {
            settings = appSettings.Mongo;
            GetCollection();
        }

        private void GetCollection()
        {
            MongoClient client = new MongoClient();
            commentCollection = client.GetDatabase(settings.Database).GetCollection<Comment>(settings.CommentCollectionName);
        }

        public bool Add(Comment comment)
        {
            var found = commentCollection.Find(new BsonDocument("_id", new BsonString(comment.ID)));
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

        public Comment GetById(string id)
        {
            var found = commentCollection.Find(new BsonDocument("_id", new BsonString(id)));
            if (found == null) return null;
            return found.FirstOrDefault();
        }

        public List<Comment> GetByPuzzle(string puzzleId)
        {
            return commentCollection.Find(new BsonDocument("puzzleId", new BsonString(puzzleId))).ToList();
        }

        public bool Edit(string id, string newBodyUnsanitized)
        {
            FilterDefinition<Comment> filter = Builders<Comment>.Filter.Eq("_id", id);
            UpdateDefinition<Comment> update = Builders<Comment>.Update.Set("bodyUnsanitized", newBodyUnsanitized);
            UpdateResult res = commentCollection.UpdateOne(filter, update);
            return res.IsAcknowledged && res.MatchedCount != 0;
        }

        public bool SoftDelete(string id)
        {
            FilterDefinition<Comment> filter = Builders<Comment>.Filter.Eq("_id", id);
            UpdateDefinition<Comment> update = Builders<Comment>.Update.Set("deleted", true);
            UpdateResult res = commentCollection.UpdateOne(filter, update);
            return res.IsAcknowledged && res.MatchedCount != 0;
        }
    }
}
