using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface IUserRepository
    {
        void Add(User user);

        void Update(User user);

        void Delete(User user);
    }
}
