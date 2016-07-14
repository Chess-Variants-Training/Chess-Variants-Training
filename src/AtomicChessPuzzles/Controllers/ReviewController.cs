using AtomicChessPuzzles.DbRepositories;
using AtomicChessPuzzles.Models;
using Microsoft.AspNet.Mvc;
using System.Collections.Generic;

namespace AtomicChessPuzzles.Controllers
{
    public class ReviewController : Controller
    {
        IPuzzleRepository puzzleRepository;

        public ReviewController(IPuzzleRepository _puzzleRepository)
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