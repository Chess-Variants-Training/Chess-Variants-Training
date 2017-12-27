using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface ICounterRepository
    {
        // int GetAndIncrease(string id);
        Task<int> GetAndIncreaseAsync(string id);
    }
}
