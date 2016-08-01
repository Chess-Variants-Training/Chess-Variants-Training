using ChessDotNet;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AtomicChessPuzzles.Models
{
    public class SubmittedMoveResponse
    {
        public bool Success
        {
            get;
            set;
        }

        public string Error
        {
            get;
            set;
        }

        public const int CORRECT_AND_FINISHED = 1;
        public const int CORRECT_AND_CONTINUE = 0;
        public const int INCORRECT = -1;
        public const int INVALID_MOVE = -2;
        public int Correct
        {
            get;
            set;
        }

        public string FEN
        {
            get;
            set;
        }

        public string ExplanationSafe
        {
            get;
            set;
        }

        public string Check
        {
            get;
            set;
        }

        public string Play
        {
            get;
            set;
        }

        public string FenAfterPlay
        {
            get;
            set;
        }

        public string CheckAfterAutoMove
        {
            get;
            set;
        }

        public ReadOnlyCollection<Move> Moves
        {
            get;
            set;
        }

        public List<string> ReplayFENs
        {
            get;
            set;
        }

        public List<string> ReplayMoves
        {
            get;
            set;
        }

        public List<string> ReplayChecks
        {
            get;
            set;
        }

        public string[] LastMove
        {
            get;
            set;
        }

        public bool DrawAfterAutoMove
        {
            get;
            set;
        }
    }
}
