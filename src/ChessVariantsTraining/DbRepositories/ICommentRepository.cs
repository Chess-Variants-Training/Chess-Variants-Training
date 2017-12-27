using ChessVariantsTraining.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface ICommentRepository
    {
        bool Add(Comment comment);
        Comment GetById(int id);
        List<Comment> GetByPuzzle(int puzzleId);
        bool Edit(int id, string newBodyUnsanitized);
        bool SoftDelete(int id);

        Task<bool> AddAsync(Comment comment);
        Task<Comment> GetByIdAsync(int id);
        Task<List<Comment>> GetByPuzzleAsync(int puzzleId);
        Task<bool> EditAsync(int id, string newBodyUnsanitized);
        Task<bool> SoftDeleteAsync(int id);
    }
}
