using ChessVariantsTraining.Models;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface ISavedLoginRepository
    {
        void Add(SavedLogin login);
        bool ContainsID(long id);
        int? AuthenticatedUser(long loginId, byte[] hashedToken);
        void Delete(long id);
        void DeleteAllOfExcept(int userId, long excludedId);
        void DeleteAllOf(int userId);

        Task AddAsync(SavedLogin login);
        Task<bool> ContainsIDAsync(long id);
        Task<int?> AuthenticatedUserAsync(long loginId, byte[] hashedToken);
        Task DeleteAsync(long id);
        Task DeleteAllOfExceptAsync(int userId, long excludedId);
        Task DeleteAllOfAsync(int userId);
    }
}
