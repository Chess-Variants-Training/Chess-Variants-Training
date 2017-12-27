using ChessVariantsTraining.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface ICommentVoteRepository
    {
        /*bool Add(CommentVote vote);
        bool Undo(string voteId);
        bool Undo(int voter, int commentId);
        bool UndoAllByVoter(int userId);
        bool ResetCommentScore(int commentId);
        int GetScoreForComment(int commentId);
        Dictionary<int, VoteType> VotesByUserOnThoseComments(int voter, List<int> commentIds);*/

        Task<bool> AddAsync(CommentVote vote);
        Task<bool> UndoAsync(string voteId);
        Task<bool> UndoAsync(int voter, int commentId);
        Task<bool> UndoAllByVoterAsync(int userId);
        Task<bool> ResetCommentScoreAsync(int commentId);
        Task<int> GetScoreForCommentAsync(int commentId);
        Task<Dictionary<int, VoteType>> VotesByUserOnThoseCommentsAsync(int voter, List<int> commentIds);
    }
}
