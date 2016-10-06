namespace ChessVariantsTraining.ViewModels
{
    public class Comment
    {
        public int ID { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public int Score { get; set; }
        public int IndentLevel { get; set; }
        public bool Deleted { get; set; }
        public int PuzzleID { get; set; }
        public string DatePosted { get; set; }

        public Comment(Models.Comment orig, int indentLevel, int score, bool deleted, string authorUsername)
        {
            ID = orig.ID;
            Body = orig.Body;
            Author = authorUsername;
            Score = score;
            IndentLevel = indentLevel;
            Deleted = deleted;
            PuzzleID = orig.PuzzleID;
            DatePosted = orig.DatePostedUtc.ToString("yyyy-MM-dd HH:mm:ss") + " (UTC)";
        }
    }
}
