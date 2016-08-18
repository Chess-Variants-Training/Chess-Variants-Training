namespace AtomicChessPuzzles.ViewModels
{
    public class Comment
    {
        public string ID { get; set; }
        public string BodySanitized { get; set; }
        public string Author { get; set; }
        public int Score { get; set; }
        public int IndentLevel { get; set; }
        public bool Deleted { get; set; }
        public string PuzzleID { get; set; }
        public string DatePosted { get; set; }

        public Comment(Models.Comment orig, int indentLevel, int score, bool deleted, string authorUsername)
        {
            ID = orig.ID;
            BodySanitized = orig.BodySanitized;
            Author = authorUsername;
            Score = score;
            IndentLevel = indentLevel;
            Deleted = deleted;
            PuzzleID = orig.PuzzleID;
            DatePosted = orig.DatePostedUtc.ToString("yyyy-MM-dd HH:mm:ss") + " (UTC)";
        }
    }
}
