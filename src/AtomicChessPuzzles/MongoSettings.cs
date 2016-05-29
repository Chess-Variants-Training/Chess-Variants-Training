using Microsoft.Extensions.Configuration;

namespace AtomicChessPuzzles
{
    public class MongoSettings
    {
        public string MongoConnectionString { get; private set; }
        public string Database { get; private set; }
        public string UserCollectionName { get; private set; }
        public string PuzzleCollectionName { get; private set; }
        public string CommentCollectionName { get; private set; }
        public string CommentVoteCollectionName { get; private set; }

        public MongoSettings()
        {
            var config = new ConfigurationBuilder().AddJsonFile("config.json").Build();
            MongoConnectionString = config.Get<string>("mongo:mongoconnectionstring", null);
            Database = config.Get<string>("mongo:database", null);
            UserCollectionName = config.Get<string>("mongo:usercollectionname");
            PuzzleCollectionName = config.Get<string>("mongo:puzzlecollectionname");
            CommentCollectionName = config.Get<string>("mongo:commentcollectionname");
            CommentVoteCollectionName = config.Get<string>("mongo:commentvotecollectionname");
        }
    }
}
