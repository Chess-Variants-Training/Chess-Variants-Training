using System.Collections.Generic;

namespace ChessVariantsTraining.ViewModels
{
    public class User
    {
        public string Username { get; private set; }
        public string About { get; private set; }
        public int PuzzlesCorrect { get; private set; }
        public int PuzzlesWrong { get; private set; }
        public List<string> Roles { get; private set; }
        public int ID { get; private set; }
        public int PuzzlesMade
        {
            get
            {
                return PuzzlesCorrect + PuzzlesWrong;
            }
        }
        public bool Closed { get; private set; }
        public long GamesPlayed { get; private set; }

        public User(string username)
        {
            Username = username;
        }

        public User(Models.User user, long gamesPlayed)
        {
            Username = user.Username;
            About = user.About;
            PuzzlesCorrect = user.PuzzlesCorrect;
            PuzzlesWrong = user.PuzzlesWrong;
            Roles = user.Roles;
            ID = user.ID;
            Closed = user.Closed;
            GamesPlayed = gamesPlayed;
        }
    }
}
