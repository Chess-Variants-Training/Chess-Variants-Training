using ChessVariantsTraining.Attributes;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Models;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ChessVariantsTraining.Controllers
{
    public class CommentController : CVTController
    {
        ICommentRepository commentRepository;
        ICommentVoteRepository commentVoteRepository;
        ICounterRepository counterRepository;
        INotificationRepository notificationRepository;
        IPuzzleRepository puzzleRepository;

        public CommentController(ICommentRepository _commentRepository,
            ICommentVoteRepository _commentVoteRepository,
            IUserRepository _userRepository,
            ICounterRepository _counterRepository,
            IPersistentLoginHandler _loginHandler,
            INotificationRepository _notificationRepository,
            IPuzzleRepository _puzzleRepository) : base(_userRepository, _loginHandler)
        {
            commentRepository = _commentRepository;
            commentVoteRepository = _commentVoteRepository;
            counterRepository = _counterRepository;
            notificationRepository = _notificationRepository;
            puzzleRepository = _puzzleRepository;
        }

        [Restricted(true, UserRole.NONE)]
        [HttpPost]
        [Route("/Comment/PostComment", Name = "PostComment")]
        public async Task<IActionResult> PostComment(string commentBody, string puzzleId)
        {
            int puzzleIdI;
            if (!int.TryParse(puzzleId, out puzzleIdI))
            {
                return Json(new { success = false, error = "Invalid puzzle ID." });
            }
            Puzzle puzzle = await puzzleRepository.GetAsync(puzzleIdI);
            bool success = false;
            Comment comment = null;
            if (puzzle != null)
            {
                comment = new Comment(await counterRepository.GetAndIncreaseAsync(Counter.COMMENT_ID), (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value, commentBody, null, puzzleIdI, false, DateTime.UtcNow);
                success = await commentRepository.AddAsync(comment);
            }
            if (success)
            {
                Notification notificationForParentAuthor = new Notification(Guid.NewGuid().ToString(), puzzle.Author, "You received a comment on your puzzle.", false,
                    string.Format("/Puzzle/{0}?comment={1}", comment.PuzzleID, comment.ID), DateTime.UtcNow);
                await notificationRepository.AddAsync(notificationForParentAuthor);
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Could not post comment." });
            }
        }

        [HttpGet]
        [Route("/Comment/ViewComments/{puzzleId:int}", Name = "ViewComments")]
        public async Task<IActionResult> ViewComments(int puzzleId)
        {
            Task<List<Comment>> commentsTask = commentRepository.GetByPuzzleAsync(puzzleId);

            Dictionary<int, VoteType> votesByCurrentUser = new Dictionary<int, VoteType>();
            bool hasCommentModerationPrivilege = false;
            int? userId = await loginHandler.LoggedInUserIdAsync(HttpContext);

            List<Comment> comments = await commentsTask;
            Task<ReadOnlyCollection<ViewModels.Comment>> roComments = new ViewModels.CommentSorter(commentVoteRepository, userRepository).OrderAsync(comments);

            if (userId.HasValue)
            {
                votesByCurrentUser = await commentVoteRepository.VotesByUserOnThoseCommentsAsync(userId.Value, comments.Select(x => x.ID).ToList());
                hasCommentModerationPrivilege = UserRole.HasAtLeastThePrivilegesOf((await userRepository.FindByIdAsync(userId.Value)).Roles, UserRole.COMMENT_MODERATOR);
            }
            Tuple<ReadOnlyCollection<ViewModels.Comment>, Dictionary<int, VoteType>, bool, bool> model = new Tuple<ReadOnlyCollection<ViewModels.Comment>, Dictionary<int, VoteType>, bool, bool>(await roComments,
                votesByCurrentUser,
                hasCommentModerationPrivilege,
                userId.HasValue);
            return View("Comments", model);
        }

        [Restricted(true, UserRole.NONE)]
        [HttpPost]
        [Route("/Comment/Upvote")]
        public async Task<IActionResult> Upvote(string commentId)
        {
            int commentIdI;
            if (!int.TryParse(commentId, out commentIdI))
            {
                return Json(new { success = false, error = "Invalid comment ID." });
            }

            Comment cmt = await commentRepository.GetByIdAsync(commentIdI);
            if (cmt == null)
            {
                return Json(new { success = false, error = "Comment not found." });
            }
            if (cmt.Author == (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value)
            {
                return Json(new { success = false, error = "You can't vote for your own comments." });
            }

            bool success = await commentVoteRepository.AddAsync(new CommentVote(VoteType.Upvote, (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value, commentIdI));
            if (success)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Couldn't vote. Have you already voted?" });
            }
        }

        [Restricted(true, UserRole.NONE)]
        [HttpPost]
        [Route("/Comment/Downvote")]
        public async Task<IActionResult> Downvote(string commentId)
        {
            int commentIdI;
            if (!int.TryParse(commentId, out commentIdI))
            {
                return Json(new { success = false, error = "Invalid comment ID." });
            }

            Comment cmt = await commentRepository.GetByIdAsync(commentIdI);
            if (cmt == null)
            {
                return Json(new { success = false, error = "Comment not found." });
            }
            if (cmt.Author == (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value)
            {
                return Json(new { success = false, error = "You can't vote for your own comments." });
            }

            bool success = await commentVoteRepository.AddAsync(new CommentVote(VoteType.Downvote, (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value, commentIdI));
            if (success)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Couldn't vote. Have you already voted?" });
            }
        }

        [Restricted(true, UserRole.NONE)]
        [HttpPost]
        [Route("/Comment/UndoVote")]
        public async Task<IActionResult> UndoVote(string commentId)
        {
            int commentIdI;
            if (!int.TryParse(commentId, out commentIdI))
            {
                return Json(new { success = false, error = "Invalid comment ID." });
            }

            bool success = await commentVoteRepository.UndoAsync((await loginHandler.LoggedInUserIdAsync(HttpContext)).Value, commentIdI);
            if (success)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Couldn't undo vote." });
            }
        }

        [Restricted(true, UserRole.NONE)]
        [HttpPost]
        [Route("/Comment/Reply")]
        public async Task<IActionResult> Reply(string to, string body, string puzzleId)
        {
            int parentId;
            if (!int.TryParse(to, out parentId))
            {
                return Json(new { success = false, error = "Invalid parent ID." });
            }

            int puzzleIdI;
            if (!int.TryParse(puzzleId, out puzzleIdI))
            {
                return Json(new { success = false, error = "Invalid puzzle ID." });
            }

            Comment parent = await commentRepository.GetByIdAsync(parentId);
            if (parent == null)
            {
                return Json(new { success = false, error = "Invalid parent ID." });
            }

            Comment comment = new Comment(await counterRepository.GetAndIncreaseAsync(Counter.COMMENT_ID), (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value, body, parentId, puzzleIdI, false, DateTime.UtcNow);
            bool success = await commentRepository.AddAsync(comment);
            if (success)
            {
                Notification notificationForParentAuthor = new Notification(Guid.NewGuid().ToString(), parent.Author, "You received a reply to your comment.", false,
                    string.Format("/Puzzle/{0}?comment={1}", comment.PuzzleID, comment.ID), DateTime.UtcNow);
                await notificationRepository.AddAsync(notificationForParentAuthor);
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Could not post comment." });
            }
        }

        [Restricted(true, UserRole.COMMENT_MODERATOR)]
        [HttpPost]
        [Route("/Comment/Mod/Delete")]
        public async Task<IActionResult> DeleteComment(string commentId)
        {
            int commentIdI;
            if (!int.TryParse(commentId, out commentIdI))
            {
                return Json(new { success = false, error = "Invalid comment ID." });
            }

            bool deleteSuccess = await commentRepository.SoftDeleteAsync(commentIdI);
            if (deleteSuccess)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Couldn't delete comment. Does it exist?" });
            }
        }
    }
}