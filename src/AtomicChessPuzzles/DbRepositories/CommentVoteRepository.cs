using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AtomicChessPuzzles.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace AtomicChessPuzzles.DbRepositories
{
    public class CommentVoteRepository : ICommentVoteRepository
    {
        MongoSettings settings;
        IMongoCollection<CommentVote> voteCollection;

        public CommentVoteRepository()
        {
            settings = new MongoSettings();
            GetCollection();
        }

        private void GetCollection()
        {
            MongoClient client = new MongoClient();
            voteCollection = client.GetDatabase(settings.Database).GetCollection<CommentVote>(settings.CommentVoteCollectionName);
        }

        public bool Add(CommentVote vote)
        {
            ReplaceOneResult result = voteCollection.ReplaceOne(new BsonDocument("_id", new BsonString(vote.ID)), vote, new UpdateOptions() { IsUpsert = true });
            return result.IsAcknowledged;
        }

        public bool Undo(string voteId)
        {
            DeleteResult result = voteCollection.DeleteOne(new BsonDocument("_id", new BsonString(voteId)));
            return result.IsAcknowledged && result.DeletedCount != 0;
        }

        public bool Undo(string voter, string commentId)
        {
            FilterDefinitionBuilder<CommentVote> builder = Builders<CommentVote>.Filter;
            FilterDefinition<CommentVote> filter = builder.Eq("voter", voter) & builder.Eq("affectedComment", commentId);
            DeleteResult result = voteCollection.DeleteOne(filter);
            return result.IsAcknowledged && result.DeletedCount != 0;
        }

        public bool UndoAllByVoter(string voter)
        {
            DeleteResult result = voteCollection.DeleteMany(new BsonDocument("voter", new BsonString(voter)));
            return result.IsAcknowledged;
        }

        public bool ResetCommentScore(string commentId)
        {
            DeleteResult result = voteCollection.DeleteMany(new BsonDocument("affectedComment", new BsonString(commentId)));
            return result.IsAcknowledged;
        }

        public int GetScoreForComment(string commentId)
        {
            var votes = voteCollection.Find(new BsonDocument("affectedComment", new BsonString(commentId)));
            if (votes == null || !votes.Any())
            {
                return 0;
            }
            int score = 0;
            var enumerableVotes = votes.ToEnumerable();
            foreach (CommentVote vote in enumerableVotes)
            {
                score += vote.Type == VoteType.Upvote ? 1 : -1;
            }
            return score;
        }

        public Dictionary<string, VoteType> VotesByUserOnThoseComments(string voter, List<string> commentIds)
        {
            voter = voter.ToLowerInvariant();
            FilterDefinitionBuilder<CommentVote> builder = Builders<CommentVote>.Filter;
            FilterDefinition<CommentVote> filter = builder.In("affectedComment", commentIds) & builder.Eq("voter", voter);
            var found = voteCollection.Find(filter);
            Dictionary<string, VoteType> result = new Dictionary<string, VoteType>();
            if (found == null)
            {
                return result;
            }
            var enumerable = found.ToEnumerable();
            foreach (CommentVote vote in enumerable)
            {
                result.Add(vote.AffectedComment, vote.Type);
            }
            return result;
        }
    }
}
