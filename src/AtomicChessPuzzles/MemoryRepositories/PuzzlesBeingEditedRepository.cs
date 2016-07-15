using System.Collections.Generic;
using System.Linq;
using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.MemoryRepositories
{
    public class PuzzlesBeingEditedRepository : IPuzzlesBeingEditedRepository
    {
        List<Puzzle> puzzles = new List<Puzzle>();

        public void Add(Puzzle puzzle)
        {
            puzzles.Add(puzzle);
        }

        public Puzzle Get(string id)
        {
            return puzzles.Where(x => x.ID == id).FirstOrDefault();
        }

        public void Remove(string id)
        {
            puzzles.RemoveAll(x => x.ID == id);
        }

        public bool Contains(string id)
        {
            return puzzles.Any(x => x.ID == id);
        }
    }
}
