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

            IEnumerable<Task<Comment>> currentTopLevelTask = list.Where(x => x.ParentID == parent)
                .Select(async x => new Comment(x, indentLevel, await voteRepo.GetScoreForCommentAsync(x.ID), x.Deleted, (await userRepo.FindByIdAsync(x.Author)).Username));
            IEnumerable<Comment> currentTopLevel = await Task.WhenAll(currentTopLevelTask);
            currentTopLevel = currentTopLevel.OrderByDescending(x => x.Score);
            foreach (Comment comment in currentTopLevel)
            {
                result.Add(comment);
                result.AddRange(await OrderRecursivelyAsync(list, comment.ID, indentLevel + 1));
            }

            return result;
        }
    }
}
