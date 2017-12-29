using ChessVariantsTraining.Attributes;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Models;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessVariantsTraining.Controllers
{
    [Restricted(true, UserRole.PUZZLE_REVIEWER)]
    public class ReviewController : CVTController
    {
        IPuzzleRepository puzzleRepository;
        INotificationRepository notificationRepository;

        public ReviewController(IPuzzleRepository _puzzleRepository, IUserRepository _userRepository, IPersistentLoginHandler _loginHandler, INotificationRepository _notificationRepository)
            : base(_userRepository, _loginHandler)
        {
            puzzleRepository = _puzzleRepository;
            notificationRepository = _notificationRepository;
        }

        [Route("/Review")]
        public async Task<IActionResult> Index()
        {
            List<Puzzle> inReview = await puzzleRepository.InReviewAsync();
            Dictionary<int, User> users = await userRepository.FindByIdsAsync(inReview.Select(x => x.Author));
            return View(new Tuple<List<Puzzle>, Dictionary<int, User>>(inReview, users));
        }

        [HttpPost]
        [Route("/Review/Approve/{id:int}")]
        public async Task<IActionResult> Approve(int id)
        {
            if (await puzzleRepository.ApproveAsync(id, (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value))
            {
                Puzzle approved = await puzzleRepository.GetAsync(id);
                Notification notif = new Notification(Guid.NewGuid().ToString(), approved.Author, "Your puzzle has been approved!", false, Url.Action("TrainId", "Puzzle", new { id }), DateTime.UtcNow);
                await notificationRepository.AddAsync(notif);
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Approval failed." });
            }
        }

        [HttpPost]
        [Route("/Review/Reject/{id:int}")]
        public async Task<IActionResult> Reject(int id)
        {
            if (await puzzleRepository.RejectAsync(id, (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value))
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