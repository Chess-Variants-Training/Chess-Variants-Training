namespace AtomicChessPuzzles.ViewModels
{
    public class User
    {
        public string Username { get; private set; }
        public string About { get; private set; }
        public int PuzzlesCorrect { get; private set; }
        public int PuzzlesWrong { get; private set; }
        public int PuzzlesMade
        {
            get
            {
                return PuzzlesCorrect + PuzzlesWrong;
            }
        }

        public float PercentageCorrect
        {
            get
            {
                if (PuzzlesMade == 0) return 0;
                return PuzzlesCorrect / (float)PuzzlesMade;
            }
        }

        public User(string username)
        {
            Username = username;
        }

        public User(Models.User user)
        {
            Username = user.Username;
            About = user.About;
            PuzzlesCorrect = user.PuzzlesCorrect;
            PuzzlesWrong = user.PuzzlesWrong;
        }
    }
}
