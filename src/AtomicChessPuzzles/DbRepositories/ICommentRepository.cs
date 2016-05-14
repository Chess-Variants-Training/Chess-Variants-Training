using AtomicChessPuzzles.Models;
using System.Collections.Generic;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface ICommentRepository
    {
        bool Add(Comment comment);

        Comment GetById(string id);

        List<Comment> GetByPuzzle(string puzzleId);

        void Edit(string id, string newBodyUnsanitized);

        void AdjustScore(string id, int scoreChange);
    }
}
