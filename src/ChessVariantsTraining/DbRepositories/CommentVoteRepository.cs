using System.Collections.Generic;
using System.Linq;
using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public class CommentVoteRepository : ICommentVoteRepository
    {
        MongoSettings settings;
        IMongoCollection<CommentVote> voteCollection;

        public CommentVoteRepository(IOptions<Settings> appSettings)
        {
            settings = appSettings.Value.Mongo;
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

        public bool Undo(int voter, int commentId)
        {
            FilterDefinitionBuilder<CommentVote> builder = Builders<CommentVote>.Filter;
            FilterDefinition<CommentVote> filter = builder.Eq("voter", voter) & builder.Eq("affectedComment", commentId);
            DeleteResult result = voteCollection.DeleteOne(filter);
            return result.IsAcknowledged && result.DeletedCount != 0;
        }

        public bool UndoAllByVoter(int voter)
        {
            DeleteResult result = voteCollection.DeleteMany(new BsonDocument("voter", new BsonInt32(voter)));
            return result.IsAcknowledged;
        }

        public bool ResetCommentScore(int commentId)
        {
            DeleteResult result = voteCollection.DeleteMany(new BsonDocument("affectedComment", new BsonInt32(commentId)));
            return result.IsAcknowledged;
        }

        public int GetScoreForComment(int commentId)
        {
            var votes = voteCollection.Find(new BsonDocument("affectedComment", new BsonInt32(commentId)));
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

        public Dictionary<int, VoteType> VotesByUserOnThoseComments(int voter, List<int> commentIds)
        {
            FilterDefinitionBuilder<CommentVote> builder = Builders<CommentVote>.Filter;
            FilterDefinition<CommentVote> filter = builder.In("affectedComment", commentIds) & builder.Eq("voter", voter);
            var found = voteCollection.Find(filter);
            Dictionary<int, VoteType> result = new Dictionary<int, VoteType>();
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

        public async Task<bool> AddAsync(CommentVote vote)
        {
            ReplaceOneResult result = await voteCollection.ReplaceOneAsync(new BsonDocument("_id", new BsonString(vote.ID)), vote, new UpdateOptions() { IsUpsert = true });
            return result.IsAcknowledged;
        }

        public async Task<bool> UndoAsync(string voteId)
        {
            DeleteResult result = await voteCollection.DeleteOneAsync(new BsonDocument("_id", new BsonString(voteId)));
            return result.IsAcknowledged;
        }

        public async Task<bool> UndoAsync(int voter, int commentId)
        {
            FilterDefinitionBuilder<CommentVote> builder = Builders<CommentVote>.Filter;
            FilterDefinition<CommentVote> filter = builder.Eq("voter", voter) & builder.Eq("affectedComment", commentId);
            DeleteResult result = await voteCollection.DeleteOneAsync(filter);
            return result.IsAcknowledged && result.DeletedCount != 0;
        }

        public async Task<bool> UndoAllByVoterAsync(int voter)
        {
            DeleteResult result = await voteCollection.DeleteManyAsync(new BsonDocument("voter", new BsonInt32(voter)));
            return result.IsAcknowledged;
        }

        public async Task<bool> ResetCommentScoreAsync(int commentId)
        {
            DeleteResult result = await voteCollection.DeleteManyAsync(new BsonDocument("affectedComment", new BsonInt32(commentId)));
            return result.IsAcknowledged;
        }

        public async Task<int> GetScoreForCommentAsync(int commentId)
        {
            var votes = voteCollection.Find(new BsonDocument("affectedComment", new BsonInt32(commentId)));
            int score = 0;
            await votes.ForEachAsync(x => score += x.Type == VoteType.Upvote ? 1 : -1);
            return score;
        }

        public async Task<Dictionary<int, VoteType>> VotesByUserOnThoseCommentsAsync(int voter, List<int> commentIds)
        {
            FilterDefinitionBuilder<CommentVote> builder = Builders<CommentVote>.Filter;
            FilterDefinition<CommentVote> filter = builder.In("affectedComment", commentIds) & builder.Eq("voter", voter);
            var found = voteCollection.Find(filter);
            Dictionary<int, VoteType> result = new Dictionary<int, VoteType>();
            if (found == null)
            {
                return result;
            }
            await found.ForEachAsync(vote => result.Add(vote.AffectedComment, vote.Type));
            return result;
        }
    }
}
