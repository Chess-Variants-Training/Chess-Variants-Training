using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ChessVariantsTraining.DbRepositories;

namespace ChessVariantsTraining.ViewModels
{
    public class CommentSorter
    {
        ICommentVoteRepository voteRepo;
        IUserRepository userRepo;

        public CommentSorter(ICommentVoteRepository _voteRepo, IUserRepository _userRepo)
        {
            voteRepo = _voteRepo;
            userRepo = _userRepo;
        }

        public async Task<ReadOnlyCollection<Comment>> OrderAsync(List<Models.Comment> list)
        {
            return new ReadOnlyCollection<Comment>(await OrderRecursivelyAsync(list, null, 0));
        }

        async Task<List<Comment>> OrderRecursivelyAsync(List<Models.Comment> list, int? parent, int indentLevel)
        {
            List<Comment> result = new List<Comment>();

            IEnumerable<Task<Comment>> currentTopLevel = list.Where(x => x.ParentID == parent)
                .Select(async x => new Comment(x, indentLevel, await voteRepo.GetScoreForCommentAsync(x.ID), x.Deleted, (await userRepo.FindByIdAsync(x.Author)).Username))
                .OrderByDescending(async x => (await x).Score);
            foreach (Task<Comment> commentTask in currentTopLevel)
            {
                Comment comment = await commentTask;
                result.Add(comment);
                result.AddRange(await OrderRecursivelyAsync(list, comment.ID, indentLevel + 1));
            }

            return result;
        }
    }
}
