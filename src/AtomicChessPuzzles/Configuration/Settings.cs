using Microsoft.Extensions.Configuration;

namespace AtomicChessPuzzles.Configuration
{
    public class Settings : ISettings
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

        public Settings()
        {
            var config = new ConfigurationBuilder().AddJsonFile("config.json").Build();

            Mongo = new MongoSettings();
            Mongo.MongoConnectionString = config.Get<string>("mongo:mongoConnectionString", null);
            Mongo.Database = config.Get<string>("mongo:database", null);
            Mongo.UserCollectionName = config.Get<string>("mongo:userCollectionName");
            Mongo.PuzzleCollectionName = config.Get<string>("mongo:puzzleCollectionName");
            Mongo.CommentCollectionName = config.Get<string>("mongo:commentCollectionName");
            Mongo.CommentVoteCollectionName = config.Get<string>("mongo:commentVoteCollectionName");
            Mongo.ReportCollectionName = config.Get<string>("mongo:reportCollectionName");
            Mongo.PositionCollectionName = config.Get<string>("mongo:positionCollectionName");
            Mongo.TimedTrainingScoreCollectionName = config.Get<string>("mongo:timedTrainingScoreCollectionName");

            TimedTrainingSessionAutoAcknowledgerDelay = config.Get<int>("timedTrainingSessionAutoAcknowledgerDelay");
        }
    }
}
