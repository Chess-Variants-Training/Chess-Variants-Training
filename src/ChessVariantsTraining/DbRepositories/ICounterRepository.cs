using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface ICounterRepository
    {
        Task<int> GetAndIncreaseAsync(string id);
    }
}
