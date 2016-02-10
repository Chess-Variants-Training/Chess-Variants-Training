using System.Collections.Generic;
using ChessDotNet.Variants.Atomic;

namespace AtomicChessPuzzles.Models
{
    public class Puzzle
    {
        public AtomicChessGame Game
        {
            get;
            set;
        }

        public List<string> Solutions
        {
            get;
            set;
        }

        public string ID
        {
            get;
            set;
        }

        public string InitialFen
        {
            get;
            set;
        }
    }
}
