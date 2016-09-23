namespace ChessVariantsTraining.Configuration
{
    public class EmailSettings
    {
        public bool RequireEmailVerification { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string Password { get; set; }
        public string SenderName { get; set; }
        public string FromAddress { get; set; }
    }
}
