namespace ChessVariantsTraining.Services
{
    public interface IUserVerifier
    {
        void SendVerificationEmailTo(int userId);

        bool Verify(int userId, int verificationCode);
    }
}
