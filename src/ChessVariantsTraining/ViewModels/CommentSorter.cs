using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ChessVariantsTraining.DbRepositories;

namespace ChessVariantsTraining.ViewModels
{
    public class CommentSorter
    {
        public ReadOnlyCollection<Comment> Ordered
        {
            get;
            private set;
        }

        ICommentVoteRepository voteRepo;
        IUserRepository userRepo;

        public CommentSorter(List<Models.Comment> list, ICommentVoteRepository _voteRepo, IUserRepository _userRepo)
        {
            voteRepo = _voteRepo;
            userRepo = _userRepo;
            Ordered = new ReadOnlyCollection<Comment>(OrderRecursively(list, null, 0));
        }

        List<Comment> OrderRecursively(List<Models.Comment> list, int? parent, int indentLevel)
        {
            List<Comment> result = new List<Comment>();

            IEnumerable<Comment> currentTopLevel = list.Where(x => x.ParentID == parent).Select(x => new Comment(x, indentLevel, voteRepo.GetScoreForComment(x.ID), x.Deleted, userRepo.FindById(x.Author).Username)).OrderByDescending(x => x.Score);
            foreach (Comment comment in currentTopLevel)
            {
                result.Add(comment);
                result.AddRange(OrderRecursively(list, comment.ID, indentLevel + 1));
            }

            return result;
        }
    }
}
