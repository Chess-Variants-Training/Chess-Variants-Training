using System.Collections.Generic;
using System.Linq;
using ChessVariantsTraining.Models;

namespace ChessVariantsTraining.MemoryRepositories
{
    public class PuzzlesBeingEditedRepository : IPuzzlesBeingEditedRepository
    {
        List<Puzzle> puzzles = new List<Puzzle>();

        public void Add(Puzzle puzzle)
        {
            puzzles.Add(puzzle);
        }

        public Puzzle Get(int id)
        {
            return puzzles.Where(x => x.ID == id).FirstOrDefault();
        }

        public void Remove(int id)
        {
            puzzles.RemoveAll(x => x.ID == id);
        }

        public bool Contains(int id)
        {
            return puzzles.Any(x => x.ID == id);
        }
    }
}
