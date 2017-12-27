using ChessVariantsTraining.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface ICommentRepository
    {
        Task<bool> AddAsync(Comment comment);
        Task<Comment> GetByIdAsync(int id);
        Task<List<Comment>> GetByPuzzleAsync(int puzzleId);
        Task<bool> EditAsync(int id, string newBodyUnsanitized);
        Task<bool> SoftDeleteAsync(int id);
    }
}
