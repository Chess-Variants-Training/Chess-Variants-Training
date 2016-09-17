using ChessVariantsTraining.Attributes;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Models;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ChessVariantsTraining.Controllers
{
    public class CommentController : RestrictedController
    {
        ICommentRepository commentRepository;
        ICommentVoteRepository commentVoteRepository;
        ICounterRepository counterRepository;
        INotificationRepository notificationRepository;

        public CommentController(ICommentRepository _commentRepository, ICommentVoteRepository _commentVoteRepository,
                                 IUserRepository _userRepository, ICounterRepository _counterRepository, IPersistentLoginHandler _loginHandler,
                                 INotificationRepository _notificationRepository) : base(_userRepository, _loginHandler)
        {
            commentRepository = _commentRepository;
            commentVoteRepository = _commentVoteRepository;
            counterRepository = _counterRepository;
            notificationRepository = _notificationRepository;
        }

        [Restricted(true, UserRole.NONE)]
        [HttpPost]
        [Route("/Comment/PostComment", Name = "PostComment")]
        public IActionResult PostComment(string commentBody, string puzzleId)
        {
            int puzzleIdI;
            if (!int.TryParse(puzzleId, out puzzleIdI))
            {
                return Json(new { success = false, error = "Invalid puzzle ID." });
            }
            Comment comment = new Comment(counterRepository.GetAndIncrease(Counter.COMMENT_ID), loginHandler.LoggedInUserId(HttpContext).Value, commentBody, null, puzzleIdI, false, DateTime.UtcNow);
            bool success = commentRepository.Add(comment);
            if (success)
            {
                return Json(new { success = true, bodySanitized = comment.BodySanitized });
            }
            else
            {
                return Json(new { success = false, error = "Could not post comment." });
            }
        }

        [HttpGet]
        [Route("/Comment/ViewComments/{puzzleId:int}", Name = "ViewComments")]
        public IActionResult ViewComments(int puzzleId)
        {
            List<Comment> comments = commentRepository.GetByPuzzle(puzzleId);
            ReadOnlyCollection<ViewModels.Comment> roComments = new ViewModels.CommentSorter(comments, commentVoteRepository, userRepository).Ordered;
            Dictionary<int, VoteType> votesByCurrentUser = new Dictionary<int, VoteType>();
            bool hasCommentModerationPrivilege = false;
            int? userId = loginHandler.LoggedInUserId(HttpContext);
            if (userId.HasValue)
            {
                votesByCurrentUser = commentVoteRepository.VotesByUserOnThoseComments(userId.Value, comments.Select(x => x.ID).ToList());
                hasCommentModerationPrivilege = UserRole.HasAtLeastThePrivilegesOf(userRepository.FindById(userId.Value).Roles, UserRole.COMMENT_MODERATOR);
            }
            Tuple<ReadOnlyCollection<ViewModels.Comment>, Dictionary<int, VoteType>, bool, bool> model = new Tuple<ReadOnlyCollection<ViewModels.Comment>, Dictionary<int, VoteType>, bool, bool>(roComments, votesByCurrentUser, hasCommentModerationPrivilege, loginHandler.LoggedInUserId(HttpContext).HasValue);
            return View("Comments", model);
        }

        [Restricted(true, UserRole.NONE)]
        [HttpPost]
        [Route("/Comment/Upvote")]
        public IActionResult Upvote(string commentId)
        {
            int commentIdI;
            if (!int.TryParse(commentId, out commentIdI))
            {
                return Json(new { success = false, error = "Invalid comment ID." });
            }

            Comment cmt = commentRepository.GetById(commentIdI);
            if (cmt == null)
            {
                return Json(new { success = false, error = "Comment not found." });
            }
            if (cmt.Author == loginHandler.LoggedInUserId(HttpContext).Value)
            {
                return Json(new { success = false, error = "You can't vote for your own comments." });
            }

            bool success = commentVoteRepository.Add(new CommentVote(VoteType.Upvote, loginHandler.LoggedInUserId(HttpContext).Value, commentIdI));
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
        public IActionResult Downvote(string commentId)
        {
            int commentIdI;
            if (!int.TryParse(commentId, out commentIdI))
            {
                return Json(new { success = false, error = "Invalid comment ID." });
            }

            Comment cmt = commentRepository.GetById(commentIdI);
            if (cmt == null)
            {
                return Json(new { success = false, error = "Comment not found." });
            }
            if (cmt.Author == loginHandler.LoggedInUserId(HttpContext).Value)
            {
                return Json(new { success = false, error = "You can't vote for your own comments." });
            }

            bool success = commentVoteRepository.Add(new CommentVote(VoteType.Downvote, loginHandler.LoggedInUserId(HttpContext).Value, commentIdI));
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
        public IActionResult UndoVote(string commentId)
        {
            int commentIdI;
            if (!int.TryParse(commentId, out commentIdI))
            {
                return Json(new { success = false, error = "Invalid comment ID." });
            }

            bool success = commentVoteRepository.Undo(loginHandler.LoggedInUserId(HttpContext).Value, commentIdI);
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
        public IActionResult Reply(string to, string body, string puzzleId)
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

            Comment parent = commentRepository.GetById(parentId);
            if (parent == null)
            {
                return Json(new { success = false, error = "Invalid parent ID." });
            }

            Comment comment = new Comment(counterRepository.GetAndIncrease(Counter.COMMENT_ID), loginHandler.LoggedInUserId(HttpContext).Value, body, parentId, puzzleIdI, false, DateTime.UtcNow);
            bool success = commentRepository.Add(comment);
            if (success)
            {
                Notification notificationForParentAuthor = new Notification(Guid.NewGuid().ToString(), parent.Author, "You received a reply to your comment.", false,
                    string.Format("/Puzzle/{0}?comment={1}", comment.PuzzleID, comment.ID), DateTime.UtcNow);
                notificationRepository.Add(notificationForParentAuthor);
                return Json(new { success = true, bodySanitized = comment.BodySanitized });
            }
            else
            {
                return Json(new { success = false, error = "Could not post comment." });
            }
        }

        [Restricted(true, UserRole.COMMENT_MODERATOR)]
        [HttpPost]
        [Route("/Comment/Mod/Delete")]
        public IActionResult DeleteComment(string commentId)
        {
            int commentIdI;
            if (!int.TryParse(commentId, out commentIdI))
            {
                return Json(new { success = false, error = "Invalid comment ID." });
            }

            bool deleteSuccess = commentRepository.SoftDelete(commentIdI);
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