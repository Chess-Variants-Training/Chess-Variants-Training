namespace AtomicChessPuzzles.Services
{
    public interface IValidator
    {
        bool IsValidUsername(string username);
        bool IsValidEmail(string email);
    }
}