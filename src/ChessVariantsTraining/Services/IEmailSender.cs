namespace ChessVariantsTraining.Services
{
    public interface IEmailSender
    {
        void Send(string toAddress, string toName, string subject, string body);
    }
}
