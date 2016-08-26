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
    }
}
