using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface IUserRepository
    {
        bool Add(User user);

        void Update(User user);

        void Delete(User user);

        User FindByUsername(string name);
    }
}
