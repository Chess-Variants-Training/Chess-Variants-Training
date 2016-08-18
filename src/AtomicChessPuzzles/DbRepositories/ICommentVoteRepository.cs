using AtomicChessPuzzles.Models;
using System.Collections.Generic;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface ICommentVoteRepository
    {
        bool Add(CommentVote vote);
        bool Undo(string voteId);
        bool Undo(int voter, string commentId);
        bool UndoAllByVoter(int userId);
        bool ResetCommentScore(string commentId);
        int GetScoreForComment(string commentId);
        Dictionary<string, VoteType> VotesByUserOnThoseComments(int voter, List<string> commentIds);
    }
}
