using ChessVariantsTraining.Models;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface IPositionRepository
    {
        // TrainingPosition GetRandom(string type);

        Task<TrainingPosition> GetRandomAsync(string type);
    }
}