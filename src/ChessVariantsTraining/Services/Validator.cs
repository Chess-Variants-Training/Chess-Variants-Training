using System.Text.RegularExpressions;

namespace ChessVariantsTraining.Services
{
    public class Validator : IValidator
    {
        static readonly Regex allowedUserNameRegex = new Regex("^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);
        static readonly Regex allowedEmailRegex = new Regex(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", RegexOptions.Compiled);

        public bool IsValidUsername(string username)
        {
            return !string.IsNullOrEmpty(username) && allowedUserNameRegex.IsMatch(username);
        }

        public bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && allowedEmailRegex.IsMatch(email);
        }
    }
}