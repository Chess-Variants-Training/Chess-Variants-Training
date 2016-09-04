using ChessVariantsTraining.Models;
using System.Collections.Generic;

namespace ChessVariantsTraining.DbRepositories
{
    public interface ICommentRepository
    {
        bool Add(Comment comment);

        Comment GetById(int id);

        List<Comment> GetByPuzzle(int puzzleId);

        bool Edit(int id, string newBodyUnsanitized);

        bool SoftDelete(int id);
    }
}
