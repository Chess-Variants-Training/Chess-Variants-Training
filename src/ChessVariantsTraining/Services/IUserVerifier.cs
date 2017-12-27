using System.Threading.Tasks;

namespace ChessVariantsTraining.Services
{
    public interface IUserVerifier
    {
        Task SendVerificationEmailToAsync(int userId);
        Task<bool> VerifyAsync(int userId, int verificationCode);
    }
}
