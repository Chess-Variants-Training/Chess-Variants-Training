using AtomicChessPuzzles.DbRepositories;
using AtomicChessPuzzles.Models;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Http;
using System.Collections.Generic;

namespace AtomicChessPuzzles.Controllers
{
    public class ReviewController : Controller
    {
        IPuzzleRepository puzzleRepository;
        IUserRepository userRepository;

        public ReviewController(IPuzzleRepository _puzzleRepository, IUserRepository _userRepository)
        {
            puzzleRepository = _puzzleRepository;
            userRepository = _userRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string userId = context.HttpContext.Session.GetString("userid");
            if (userId == null)
            {
                context.Result = Json(new { success = false, error = "Not authorized." });
                return;
            }
            User user = userRepository.FindByUsername(userId);
            bool authorized = UserRole.HasAtLeastThePrivilegesOf(user.Roles, UserRole.PUZZLE_REVIEWER);
            if (!authorized)
            {
                context.Result = Json(new { success = false, error = "Not authorized." });
                return;
            }
            base.OnActionExecuting(context);
        }

        [Route("/Review")]
        public IActionResult Index()
        {
            List<Puzzle> inReview = puzzleRepository.InReview();
            return View(inReview);
        }

        [HttpPost]
        [Route("/Review/Approve/{id}")]
        public IActionResult Approve(string id)
        {
            if (puzzleRepository.Approve(id))
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Approval failed." });
            }
        }

        [HttpPost]
        [Route("/Review/Reject/{id}")]
        public IActionResult Reject(string id)
        {
            if (puzzleRepository.Reject(id))
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Rejection failed." });
            }
        }
    }
}