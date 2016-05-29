using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AtomicChessPuzzles.ViewModels
{
    public class Comment
    {
        public string ID { get; set; }
        public string BodySanitized { get; set; }
        public string Author { get; set; }
        public int Score { get; set; }
        public int IndentLevel { get; set; }

        public Comment(Models.Comment orig, int indentLevel, int score)
        {
            ID = orig.ID;
            BodySanitized = orig.BodySanitized;
            Author = orig.Author;
            Score = score;
            IndentLevel = indentLevel;
        }
    }
}
