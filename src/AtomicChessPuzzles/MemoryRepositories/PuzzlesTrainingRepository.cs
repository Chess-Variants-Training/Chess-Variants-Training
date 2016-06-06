using AtomicChessPuzzles.Models;
using System.Collections.Generic;
using System.Linq;

namespace AtomicChessPuzzles.MemoryRepositories
{
    public class PuzzlesTrainingRepository : IPuzzlesTrainingRepository
    {
        List<PuzzleDuringTraining> puzzles = new List<PuzzleDuringTraining>();

        public void Add(PuzzleDuringTraining puzzle)
        {
            puzzles.Add(puzzle);
        }

        public PuzzleDuringTraining Get(string id, string trainingSessionId)
        {
            return puzzles.Where(x => x.Puzzle.ID == id && x.TrainingSessionId == trainingSessionId).FirstOrDefault();
        }

        public void Remove(string id, string trainingSessionId)
        {
            puzzles.RemoveAll(x => x.Puzzle.ID == id && x.TrainingSessionId == trainingSessionId);
        }

        public bool Contains(string id, string trainingSessionId)
        {
            return puzzles.Any(x => x.Puzzle.ID == id && x.TrainingSessionId == trainingSessionId);
        }

        public bool ContainsTrainingSessionId(string trainingSessionId)
        {
            return puzzles.Any(x => x.TrainingSessionId == trainingSessionId);
        }

        public IEnumerable<PuzzleDuringTraining> GetForTrainingSessionId(string trainingSessionId)
        {
            return puzzles.Where(x => x.TrainingSessionId == trainingSessionId);
        }
    }
}
