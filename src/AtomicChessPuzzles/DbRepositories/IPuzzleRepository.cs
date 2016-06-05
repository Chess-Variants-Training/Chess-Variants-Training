using AtomicChessPuzzles.Models;
using MongoDB.Driver;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface IPuzzleRepository
    {
        bool Add(Puzzle puzzle);

        Puzzle Get(string id);

        Puzzle GetOneRandomly();

        DeleteResult Remove(string id);

        DeleteResult RemoveAllBy(string author);

        bool UpdateRating(string id, Rating newRating);
    }
}
