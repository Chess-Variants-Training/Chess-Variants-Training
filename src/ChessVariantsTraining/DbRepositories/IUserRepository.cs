using ChessVariantsTraining.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface IUserRepository
    {
        User FindById(int id);
        Task<bool> AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
        Task<User> FindByIdAsync(int id);
        Task<User> FindByUsernameAsync(string username);
        Task<User> FindByEmailAsync(string email);
        Task<Dictionary<int, User>> FindByIdsAsync(IEnumerable<int> ids);
        Task<User> FindByPasswordResetTokenAsync(string token);
    }
}
