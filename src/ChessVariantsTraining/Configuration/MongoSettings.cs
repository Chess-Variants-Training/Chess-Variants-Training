namespace ChessVariantsTraining.Configuration
{
    public class MongoSettings
    {
        public string MongoConnectionString { get; set; }
        public string Database { get; set; }
        public string UserCollectionName { get; set; }
        public string PuzzleCollectionName { get; set; }
        public string CommentCollectionName { get; set; }
        public string CommentVoteCollectionName { get; set; }
        public string ReportCollectionName { get; set; }
        public string PositionCollectionName { get; set; }
        public string TimedTrainingScoreCollectionName { get; set; }
        public string RatingCollectionName { get; set; }
        public string AttemptCollectionName { get; set; }
        public string CounterCollectionName { get; set; }
        public string SavedLoginCollectionName { get; set; }
        public string NotificationCollectionName { get; set; }
        public string GameCollectionName { get; set; }
    }
}
