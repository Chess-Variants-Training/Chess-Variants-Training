using AtomicChessPuzzles.Models;
using System.Collections.Generic;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface ICommentVoteRepository
    {
        bool Add(CommentVote vote);
        bool Undo(string voteId);
        bool Undo(int voter, int commentId);
        bool UndoAllByVoter(int userId);
        bool ResetCommentScore(int commentId);
        int GetScoreForComment(int commentId);
        Dictionary<int, VoteType> VotesByUserOnThoseComments(int voter, List<int> commentIds);
    }
}
