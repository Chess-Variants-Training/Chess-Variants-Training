using System.Collections.Generic;

namespace AtomicChessPuzzles.Models
{
    public class PuzzleDuringTraining
    {
        public Puzzle Puzzle
        {
            get;
            set;
        }

        public string TrainingSessionId
        {
            get;
            set;
        }

        public List<string> SolutionMovesToDo
        {
            get;
            set;
        }
    }
}
