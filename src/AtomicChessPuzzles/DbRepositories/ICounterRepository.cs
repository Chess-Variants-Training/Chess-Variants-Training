using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface ICounterRepository
    {
        int GetAndIncrease(string id);
    }
}
