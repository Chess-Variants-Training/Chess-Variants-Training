using ChessVariantsTraining.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface IUserRepository
    {
        bool Add(User user);
        void Update(User user);
        void Delete(User user);
        User FindById(int id);
        User FindByUsername(string username);
        User FindByEmail(string email);
        Dictionary<int, User> FindByIds(IEnumerable<int> ids);
        User FindByPasswordResetToken(string token);

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
