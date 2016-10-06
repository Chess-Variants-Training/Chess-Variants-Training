using ChessVariantsTraining.Attributes;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Models;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public IActionResult Index()
        {
            List<Puzzle> inReview = puzzleRepository.InReview();
            Dictionary<int, User> users = userRepository.FindByIds(inReview.Select(x => x.Author));
            return View(new Tuple<List<Puzzle>, Dictionary<int, User>>(inReview, users));
        }

        [HttpPost]
        [Route("/Review/Approve/{id:int}")]
        public IActionResult Approve(int id)
        {
            if (puzzleRepository.Approve(id, loginHandler.LoggedInUserId(HttpContext).Value))
            {
                Puzzle approved = puzzleRepository.Get(id);
                Notification notif = new Notification(Guid.NewGuid().ToString(), approved.Author, "Your puzzle has been approved!", false, Url.Action("TrainId", "Puzzle", new { id = id }), DateTime.UtcNow);
                notificationRepository.Add(notif);
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Approval failed." });
            }
        }

        [HttpPost]
        [Route("/Review/Reject/{id:int}")]
        public IActionResult Reject(int id)
        {
            if (puzzleRepository.Reject(id, loginHandler.LoggedInUserId(HttpContext).Value))
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