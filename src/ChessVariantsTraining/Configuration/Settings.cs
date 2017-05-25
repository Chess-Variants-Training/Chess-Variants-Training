namespace ChessVariantsTraining.Configuration
{
    public class Settings
    {
        public MongoSettings Mongo
        {
            get;
            set;
        }

        public int TimedTrainingSessionAutoAcknowledgerDelay
        {
            get;
            set;
        }

        public int AssetVersion
        {
            get;
            set;
        }

        public EmailSettings Email
        {
            get;
            set;
        }

        public string RecaptchaKey
        {
            get;
            set;
        }
    }
}
