using ChessVariantsTraining.Models;
using System.Collections.Generic;

namespace ChessVariantsTraining.DbRepositories
{
    public interface IUserRepository
    {
        bool Add(User user);

        void Update(User user);

        void Delete(User user);

        User FindById(int id);

        User FindByUsernameOrEmail(string name);

        Dictionary<int, User> FindByIds(IEnumerable<int> ids);

        User FindByPasswordResetToken(string token);
    }
}
