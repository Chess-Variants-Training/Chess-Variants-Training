using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.MemoryRepositories
{
    public interface IPuzzlesBeingEditedRepository
    {
        void Add(Puzzle puzzle);

        Puzzle Get(string id);

        void Remove(string id);

        bool Contains(string id);
    }
}
