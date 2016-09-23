using ChessVariantsTraining.Configuration;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ChessVariantsTraining.Services
{
    public class EmailSender : IEmailSender
    {
        EmailSettings settings;

        public EmailSender(IOptions<Settings> appSettings)
        {
            settings = appSettings.Value.Email;
        }

        public void Send(string toAddress, string toName, string subject, string body)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings.SenderName, settings.FromAddress));
            message.To.Add(new MailboxAddress(toName, toAddress));
            message.Subject = subject;
            message.Body = new TextPart() { Text = body };

            using (SmtpClient client = new SmtpClient())
            {
                client.Connect(settings.SmtpHost, settings.SmtpPort);
                client.Authenticate(settings.SmtpUsername, settings.Password);
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
