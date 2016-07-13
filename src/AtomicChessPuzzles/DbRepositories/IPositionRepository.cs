using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface IPositionRepository
    {
        TrainingPosition GetRandomMateInOne();
    }
}