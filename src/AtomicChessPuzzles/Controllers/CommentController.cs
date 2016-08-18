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

        public CommentController(ICommentRepository _commentRepository, ICommentVoteRepository _commentVoteRepository,
                                 IUserRepository _userRepository) : base(_userRepository)
        {
            commentRepository = _commentRepository;
            commentVoteRepository = _commentVoteRepository;
        }

        [Restricted(true, UserRole.NONE)]
        [HttpPost]
        [Route("/Comment/PostComment", Name = "PostComment")]
        public IActionResult PostComment(string commentBody, string puzzleId)
        {
            Comment comment = new Comment(Guid.NewGuid().ToString(), HttpContext.Session.GetInt32("userid").Value, commentBody, null, puzzleId, false, DateTime.UtcNow);
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
            Dictionary<string, VoteType> votesByCurrentUser = new Dictionary<string, VoteType>();
            bool hasCommentModerationPrivilege = false;
            int? userId = HttpContext.Session.GetInt32("userid");
            if (userId.HasValue)
            {
                votesByCurrentUser = commentVoteRepository.VotesByUserOnThoseComments(userId.Value, comments.Select(x => x.ID).ToList());
                hasCommentModerationPrivilege = UserRole.HasAtLeastThePrivilegesOf(userRepository.FindById(userId.Value).Roles, UserRole.COMMENT_MODERATOR);
            }
            Tuple<ReadOnlyCollection<ViewModels.Comment>, Dictionary<string, VoteType>, bool> model = new Tuple<ReadOnlyCollection<ViewModels.Comment>, Dictionary<string, VoteType>, bool>(roComments, votesByCurrentUser, hasCommentModerationPrivilege);
            return View("Comments", model);
        }

        [Restricted(true, UserRole.NONE)]
        [HttpPost]
        [Route("/Comment/Upvote")]
        public IActionResult Upvote(string commentId)
        {
            bool success = commentVoteRepository.Add(new CommentVote(VoteType.Upvote, HttpContext.Session.GetInt32("userid").Value, commentId));
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
            bool success = commentVoteRepository.Add(new CommentVote(VoteType.Downvote, HttpContext.Session.GetInt32("userid").Value, commentId));
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
            bool success = commentVoteRepository.Undo(HttpContext.Session.GetInt32("userid").Value, commentId);
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
            Comment comment = new Comment(Guid.NewGuid().ToString(), HttpContext.Session.GetInt32("userid").Value, body, to, puzzleId, false, DateTime.UtcNow);
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