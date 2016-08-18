using AtomicChessPuzzles.Attributes;
using AtomicChessPuzzles.DbRepositories;
using AtomicChessPuzzles.Models;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AtomicChessPuzzles.Controllers
{
    public class CommentController : RestrictedController
    {
        ICommentRepository commentRepository;
        ICommentVoteRepository commentVoteRepository;
        ICounterRepository counterRepository;

        public CommentController(ICommentRepository _commentRepository, ICommentVoteRepository _commentVoteRepository,
                                 IUserRepository _userRepository, ICounterRepository _counterRepository) : base(_userRepository)
        {
            commentRepository = _commentRepository;
            commentVoteRepository = _commentVoteRepository;
            counterRepository = _counterRepository;
        }

        [Restricted(true, UserRole.NONE)]
        [HttpPost]
        [Route("/Comment/PostComment", Name = "PostComment")]
        public IActionResult PostComment(string commentBody, string puzzleId)
        {
            Comment comment = new Comment(counterRepository.GetAndIncrease(Counter.COMMENT_ID), HttpContext.Session.GetInt32("userid").Value, commentBody, null, puzzleId, false, DateTime.UtcNow);
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
        [Route("/Comment/ViewComments", Name = "ViewComments")]
        public IActionResult ViewComments(string puzzleId)
        {
            if (puzzleId == null)
            {
                return View("Comments", null);
            }
            List<Comment> comments = commentRepository.GetByPuzzle(puzzleId);
            ReadOnlyCollection<ViewModels.Comment> roComments = new ViewModels.CommentSorter(comments, commentVoteRepository, userRepository).Ordered;
            Dictionary<int, VoteType> votesByCurrentUser = new Dictionary<int, VoteType>();
            bool hasCommentModerationPrivilege = false;
            int? userId = HttpContext.Session.GetInt32("userid");
            if (userId.HasValue)
            {
                votesByCurrentUser = commentVoteRepository.VotesByUserOnThoseComments(userId.Value, comments.Select(x => x.ID).ToList());
                hasCommentModerationPrivilege = UserRole.HasAtLeastThePrivilegesOf(userRepository.FindById(userId.Value).Roles, UserRole.COMMENT_MODERATOR);
            }
            Tuple<ReadOnlyCollection<ViewModels.Comment>, Dictionary<int, VoteType>, bool> model = new Tuple<ReadOnlyCollection<ViewModels.Comment>, Dictionary<int, VoteType>, bool>(roComments, votesByCurrentUser, hasCommentModerationPrivilege);
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

            bool success = commentVoteRepository.Add(new CommentVote(VoteType.Upvote, HttpContext.Session.GetInt32("userid").Value, commentIdI));
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

            bool success = commentVoteRepository.Add(new CommentVote(VoteType.Downvote, HttpContext.Session.GetInt32("userid").Value, commentIdI));
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

            bool success = commentVoteRepository.Undo(HttpContext.Session.GetInt32("userid").Value, commentIdI);
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

            Comment comment = new Comment(counterRepository.GetAndIncrease(Counter.COMMENT_ID), HttpContext.Session.GetInt32("userid").Value, body, parentId, puzzleId, false, DateTime.UtcNow);
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

        [Restricted(true, UserRole.COMMENT_MODERATOR)]
        [HttpPost]
        [Route("/Comment/Mod/Delete")]
        public IActionResult DeleteComment(string commentId)
        {
            bool deleteSuccess = commentRepository.SoftDelete(commentId);
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