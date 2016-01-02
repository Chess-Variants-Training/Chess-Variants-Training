using Microsoft.Extensions.Configuration;

namespace AtomicChessPuzzles
{
    public class MongoSettings
    {
        public MongoSettings()
        {
            var config = new ConfigurationBuilder().AddJsonFile("config.json").Build();
            MongoConnectionString = config.Get<string>("mongo:mongoconnectionstring", null);
            Database = config.Get<string>("mongo:database", null);
        }
        public string MongoConnectionString { get; set; }
        public string Database { get; set; }
    }
}
