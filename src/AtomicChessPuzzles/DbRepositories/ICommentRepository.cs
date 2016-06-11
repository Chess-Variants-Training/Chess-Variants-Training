using AtomicChessPuzzles.Models;
using System.Collections.Generic;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface ICommentRepository
    {
        bool Add(Comment comment);

        Comment GetById(string id);

        List<Comment> GetByPuzzle(string puzzleId);

        bool Edit(string id, string newBodyUnsanitized);

        bool SoftDelete(string id);
    }
}
