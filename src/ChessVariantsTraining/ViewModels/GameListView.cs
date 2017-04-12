using System.Collections.Generic;

namespace ChessVariantsTraining.ViewModels
{
    public class GameListView
    {
        public string Player { get; private set; }
        public List<LightGame> Games { get; private set; }
        public int CurrentPage { get; private set; }
        public IEnumerable<int> PagesToDisplay { get; private set; }

        public GameListView(string player, List<LightGame> games, int currentPage, IEnumerable<int> pagesToDisplay)
        {
            Player = player;
            Games = games;
            CurrentPage = currentPage;
            PagesToDisplay = pagesToDisplay;
        }
    }
}
