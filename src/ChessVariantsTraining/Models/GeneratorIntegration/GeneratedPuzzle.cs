using System.Collections.Generic;
using SolutionTree = System.Collections.Generic.Dictionary<string, ChessVariantsTraining.Models.GeneratorIntegration.GeneratedPuzzle.SolutionBranch>;

namespace ChessVariantsTraining.Models.GeneratorIntegration
{
    public class GeneratedPuzzle
    {
        public class Player
        {
            public string Name { get; set; }
            public string Elo { get; set; }

            public override string ToString()
            {
                return string.Format("{0} ({1})", Name, Elo);
            }
        }

        public class SolutionBranch
        {
            public string Move { get; set; }
            public Variation Variation { get; set; }
        }

        public class Variation
        {
            public string Response { get; set; }
            public SolutionTree Node { get; set; }
        }

        public string FEN { get; set; }
        public string Site { get; set; }
        public Player White { get; set; }
        public Player Black { get; set; }
        public int Depth { get; set; }
        public SolutionTree Solution { get; set; }

        List<string> FlattenSolutionTree(SolutionTree tree)
        {
            List<string> solutions = new List<string>();

            foreach (KeyValuePair<string, SolutionBranch> solutionKvp in tree)
            {
                SolutionBranch branch = solutionKvp.Value;

                string common = TranslateMove(branch.Move);

                if (branch.Variation.Response == null)
                {
                    solutions.Add(common);
                    continue;
                }

                string commonResponse = TranslateMove(branch.Variation.Response);

                List<string> deeper = FlattenSolutionTree(branch.Variation.Node);
                if (deeper.Count > 0)
                {
                    foreach (string v in deeper)
                    {
                        solutions.Add(string.Concat(common, " ", commonResponse, " ", v));
                    }
                }
                else
                {
                    solutions.Add(string.Concat(common, " ", commonResponse));
                }
            }

            return solutions;
        }

        public List<string> FlattenSolution()
        {
            return FlattenSolutionTree(Solution);
        }

        string TranslateMove(string move)
        {
            if (move.Contains("@")) return move;
            else
            {
                if (move.Length == 4)
                {
                    return move.Substring(0, 2) + "-" + move.Substring(2, 2);
                }
                else
                {
                    return move.Substring(0, 2) + "-" + move.Substring(2, 2) + "=" + char.ToUpperInvariant(move[move.Length - 1]);
                }
            }
        }
    }
}
