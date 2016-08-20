using ChessVariantsTraining.Models;

namespace ChessVariantsTraining.DbRepositories
{
    public interface IPositionRepository
    {
        TrainingPosition GetRandomMateInOne();
    }
}