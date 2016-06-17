using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using AtomicChessPuzzles.DbRepositories;
using Microsoft.AspNet.Http;
using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.Controllers
{
    public class ReportController : Controller
    {
        IReportRepository reportRepository;
        IUserRepository userRepository;

        public ReportController(IReportRepository _reportRepository, IUserRepository _userRepository)
        {
            reportRepository = _reportRepository;
            userRepository = _userRepository;
        }

        [Route("/Report/List/Comments")]
        public IActionResult ListCommentReports()
        {
            string userId = HttpContext.Session.GetString("userid");
            if (userId == null)
            {
                Response.StatusCode = 403;
                return Json(new { success = false, error = "You have no access." });
            }
            User user = userRepository.FindByUsername(userId);
            if (UserRole.HasAtLeastThePrivilegesOf(user.Roles, UserRole.COMMENT_MODERATOR))
            {
                return View("List", reportRepository.GetByType("Comment"));
            }
            else
            {
                Response.StatusCode = 403;
                return Json(new { success = false, error = "You have no access." });
            }
        }

        [HttpPost]
        [Route("/Report/Submit/{type}")]
        public IActionResult SubmitReport(string type, string item)
        {
            if (type != "Comment")
            {
                return Json(new { success = false, error = "Unknown report type." });
            }
            Report report = new Report(Guid.NewGuid().ToString(), type, HttpContext.Session.GetString("username") ?? "Anonymous", item, "Inappropriate", "This comment has been reported", false, null);
            // TODO: allow giving a reason and reason explanation
            if (reportRepository.Add(report))
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Reporting failed." });
            }
        }
    }
}
