using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface IAttemptRepository
    {
        void Add(Attempt attempt);
    }
}
