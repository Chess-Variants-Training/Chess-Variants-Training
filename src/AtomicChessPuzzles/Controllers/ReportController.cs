using System;
using System.Linq;
using Microsoft.AspNet.Mvc;
using AtomicChessPuzzles.DbRepositories;
using Microsoft.AspNet.Http;
using AtomicChessPuzzles.Attributes;
using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.Controllers
{
    public class ReportController : RestrictedController
    {
        IReportRepository reportRepository;

        public static readonly string[] ValidCommentReportReasons = new string[] { "Offensive", "Spam", "Off-topic", "Other" };

        public ReportController(IReportRepository _reportRepository, IUserRepository _userRepository) : base(_userRepository)
        {
            reportRepository = _reportRepository;
        }

        [Route("/Report/List/Comments")]
        [Restricted(true, UserRole.COMMENT_MODERATOR)]
        public IActionResult ListCommentReports()
        {
            return View("List", reportRepository.GetByType("Comment"));
        }

        [HttpPost]
        [Route("/Report/Submit/{type}")]
        public IActionResult SubmitReport(string type, string item, string reason, string reasonExplanation)
        {
            if (type != "Comment")
            {
                return Json(new { success = false, error = "Unknown report type." });
            }
            if (!ValidCommentReportReasons.Contains(reason))
            {
                return Json(new { success = false, error = "Invalid reason" });
            }
            Report report = new Report(Guid.NewGuid().ToString(), type, HttpContext.Session.GetString("username") ?? "Anonymous", item, reason, reasonExplanation, false, null);
            if (reportRepository.Add(report))
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Reporting failed." });
            }
        }

        [HttpGet]
        [Route("/Report/Dialog/Comment")]
        public IActionResult CommentReportDialog()
        {
            return View();
        }
    }
}
