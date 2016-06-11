using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AtomicChessPuzzles.DbRepositories;

namespace AtomicChessPuzzles.ViewModels
{
    public class CommentSorter
    {
        public ReadOnlyCollection<Comment> Ordered
        {
            get;
            private set;
        }

        ICommentVoteRepository voteRepo;

        public CommentSorter(List<Models.Comment> list, ICommentVoteRepository _voteRepo)
        {
            voteRepo = _voteRepo;
            Ordered = new ReadOnlyCollection<Comment>(OrderRecursively(list, null, 0));
        }

        List<Comment> OrderRecursively(List<Models.Comment> list, string parent, int indentLevel)
        {
            List<Comment> result = new List<Comment>();

            IEnumerable<Comment> currentTopLevel = list.Where(x => x.ParentID == parent).Select(x => new Comment(x, indentLevel, voteRepo.GetScoreForComment(x.ID), x.Deleted)).OrderByDescending(x => x.Score);
            foreach (Comment comment in currentTopLevel)
            {
                result.Add(comment);
                result.AddRange(OrderRecursively(list, comment.ID, indentLevel + 1));
            }

            return result;
        }
    }
}
