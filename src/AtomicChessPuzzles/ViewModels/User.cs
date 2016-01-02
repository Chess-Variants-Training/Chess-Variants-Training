namespace AtomicChessPuzzles.ViewModels
{
    public class User
    {
        public string Username { get; private set; }

        public User(string username)
        {
            Username = username;
        }

        public User(Models.User user)
        {
            Username = user.Username;
        }
    }
}
