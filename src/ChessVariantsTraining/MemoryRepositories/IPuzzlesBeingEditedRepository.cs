using ChessVariantsTraining.Models;

namespace ChessVariantsTraining.MemoryRepositories
{
    public interface IPuzzlesBeingEditedRepository
    {
        void Add(Puzzle puzzle);

        Puzzle Get(int id);

        void Remove(int id);

        bool Contains(int id);
    }
}
