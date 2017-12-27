using ChessVariantsTraining.Models;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface IPositionRepository
    {
        Task<TrainingPosition> GetRandomAsync(string type);
    }
}