using ChessVariantsTraining.Models;
using System.Collections.Generic;

namespace ChessVariantsTraining.ViewModels
{
    public class PuzzleHistoryView
    {
        public IEnumerable<int> Pages
        {
            get;
            private set;
        }

        public List<Attempt> Attempts
        {
            get;
            private set;
        }

        public int CurrentPage
        {
            get;
            private set;
        }

        public PuzzleHistoryView(IEnumerable<int> pages, List<Attempt> attempts, int currentPage)
        {
            Pages = pages;
            Attempts = attempts;
            CurrentPage = currentPage;
        }
    }
}
