namespace ChessVariantsTraining.Services
{
    public interface IValidator
    {
        bool IsValidUsername(string username);
        bool IsValidEmail(string email);
    }
}