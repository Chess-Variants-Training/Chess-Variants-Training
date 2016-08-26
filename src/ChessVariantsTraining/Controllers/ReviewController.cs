using ChessVariantsTraining.Attributes;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Models;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ChessVariantsTraining.Controllers
{
    [Restricted(true, UserRole.PUZZLE_REVIEWER)]
    public class ReviewController : RestrictedController
    {
        IPuzzleRepository puzzleRepository;

        public ReviewController(IPuzzleRepository _puzzleRepository, IUserRepository _userRepository, IPersistentLoginHandler _loginHandler) : base(_userRepository, _loginHandler)
        {
            puzzleRepository = _puzzleRepository;
        }

        [Route("/Review")]
        public IActionResult Index()
        {
            List<Puzzle> inReview = puzzleRepository.InReview();
            return View(inReview);
        }

        [HttpPost]
        [Route("/Review/Approve/{id:int}")]
        public IActionResult Approve(int id)
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
        [Route("/Review/Reject/{id:int}")]
        public IActionResult Reject(int id)
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