using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface IUserRepository
    {
        bool Add(User user);

        void Update(User user);

        void Delete(User user);

        User FindById(int id);

        User FindByUsername(string name);
    }
}
