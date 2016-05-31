using AtomicChessPuzzles.Models;
using System.Collections.Generic;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface ICommentVoteRepository
    {
        bool Add(CommentVote vote);
        bool Undo(string voteId);
        bool Undo(string voter, string commentId);
        bool UndoAllByVoter(string userId);
        bool ResetCommentScore(string commentId);
        int GetScoreForComment(string commentId);
        Dictionary<string, VoteType> VotesByUserOnThoseComments(string voter, List<string> commentIds);
    }
}
