using System.Text.RegularExpressions;

namespace AtomicChessPuzzles.Services
{
    public class Validator : IValidator
    {
        static readonly Regex allowedUserNameRegex = new Regex("^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);
        static readonly Regex allowedEmailRegex = new Regex(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", RegexOptions.Compiled);

        public bool IsValidUsername(string username)
        {
            return allowedUserNameRegex.IsMatch(username);
        }

        public bool IsValidEmail(string email)
        {
            return allowedEmailRegex.IsMatch(email);
        }
    }
}