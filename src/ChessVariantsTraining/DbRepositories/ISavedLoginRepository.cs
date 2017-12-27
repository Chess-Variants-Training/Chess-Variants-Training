using ChessVariantsTraining.Models;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface ISavedLoginRepository
    {
        bool ContainsID(long id);
        int? AuthenticatedUser(long loginId, byte[] hashedToken);

        Task AddAsync(SavedLogin login);
        Task<bool> ContainsIDAsync(long id);
        Task<int?> AuthenticatedUserAsync(long loginId, byte[] hashedToken);
        Task DeleteAsync(long id);
        Task DeleteAllOfExceptAsync(int userId, long excludedId);
        Task DeleteAllOfAsync(int userId);
    }
}
