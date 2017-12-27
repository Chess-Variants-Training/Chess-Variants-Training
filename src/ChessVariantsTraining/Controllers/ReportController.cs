using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Attributes;
using ChessVariantsTraining.Models;
using ChessVariantsTraining.Services;
using System.Threading.Tasks;

namespace ChessVariantsTraining.Controllers
{
    public class ReportController : CVTController
    {
        IReportRepository reportRepository;

        public static readonly string[] ValidCommentReportReasons = new string[] { "Offensive", "Spam", "Off-topic", "Other" };
        public static readonly string[] ValidPuzzleReportReasons = new string[] { "Inaccurate", "Too many options", "Missing answer" };
        public static readonly string[] ValidReportJudgements = new string[] { "helpful", "declined" };

        static readonly Dictionary<string, string[]> validReasonsForType = new Dictionary<string, string[]>()
        {
            { "Comment", ValidCommentReportReasons },
            { "Puzzle", ValidPuzzleReportReasons }
        };

        public ReportController(IReportRepository _reportRepository, IUserRepository _userRepository, IPersistentLoginHandler _loginHandler) : base(_userRepository, _loginHandler)
        {
            reportRepository = _reportRepository;
        }

        [Route("/Report/List/All")]
        [Restricted(true, UserRole.COMMENT_MODERATOR, UserRole.PUZZLE_EDITOR)]
        public async Task<IActionResult> ListAll()
        {
            User user = await userRepository.FindByIdAsync((await loginHandler.LoggedInUserIdAsync(HttpContext)).Value);
            List<string> roles = user.Roles;
            List<string> types = new List<string>();
            if (UserRole.HasAtLeastThePrivilegesOf(roles, UserRole.COMMENT_MODERATOR))
            {
                types.Add("Comment");
            }
            if (UserRole.HasAtLeastThePrivilegesOf(roles, UserRole.PUZZLE_EDITOR))
            {
                types.Add("Puzzle");
            }
            List<Report> reports = await reportRepository.GetUnhandledByTypesAsync(types);
            Dictionary<int, User> users = await userRepository.FindByIdsAsync(reports.Select(x => x.Reporter));
            return View("List", new Tuple<List<Report>, Dictionary<int, User>>(reports, users));
        }

        [Route("/Report/List/Comments")]
        [Restricted(true, UserRole.COMMENT_MODERATOR)]
        public async Task<IActionResult> ListCommentReports()
        {
            List<Report> reports = await reportRepository.GetUnhandledByTypeAsync("Comment");
            Dictionary<int, User> users = await userRepository.FindByIdsAsync(reports.Select(x => x.Reporter));
            return View("List", new Tuple<List<Report>, Dictionary<int, User>>(reports, users));
        }

        [Route("/Report/List/Puzzles")]
        [Restricted(true, UserRole.PUZZLE_EDITOR)]
        public async Task<IActionResult> ListPuzzleReports()
        {
            List<Report> reports = await reportRepository.GetUnhandledByTypeAsync("Comment");
            Dictionary<int, User> users = await userRepository.FindByIdsAsync(reports.Select(x => x.Reporter));
            return View("List", new Tuple<List<Report>, Dictionary<int, User>>(reports, users));
        }

        [HttpPost]
        [Route("/Report/Submit/{type}")]
        [Restricted(true, UserRole.NONE)]
        public async Task<IActionResult> SubmitReport(string type, string item, string reason, string reasonExplanation)
        {
            string[] validTypes = new string[] { "Comment", "Puzzle" };
            if (!validTypes.Contains(type))
            {
                return Json(new { success = false, error = "Unknown report type." });
            }
            if (!validReasonsForType[type].Contains(reason))
            {
                return Json(new { success = false, error = "Invalid reason" });
            }
            Report report = new Report(Guid.NewGuid().ToString(), type, (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value, item, reason, reasonExplanation, false, null);
            if (await reportRepository.AddAsync(report))
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
        [Restricted(true, UserRole.NONE)]
        public IActionResult CommentReportDialog()
        {
            return View();
        }

        [HttpGet]
        [Route("/Report/Dialog/Puzzle")]
        [Restricted(true, UserRole.NONE)]
        public IActionResult PuzzleReportDialog()
        {
            return View();
        }

        [HttpPost]
        [Route("/Report/Handle")]
        [Restricted(true, UserRole.COMMENT_MODERATOR, UserRole.PUZZLE_EDITOR)]
        public async Task<IActionResult> HandleReport(string id, string judgement)
        {
            if (!ValidReportJudgements.Contains(judgement))
            {
                return Json(new { success = false, error = "Invalid report judgement." });
            }
            Report report = await reportRepository.GetByIdAsync(id);
            if (report == null)
            {
                return Json(new { success = false, error = "Report not found." });
            }
            if (report.Handled)
            {
                return Json(new { success = false, error = "That report got handled already." });
            }
            User handler = await loginHandler.LoggedInUserAsync(HttpContext);
            if ((report.Type == "Comment" && !UserRole.HasAtLeastThePrivilegesOf(handler.Roles, UserRole.COMMENT_MODERATOR))
                || (report.Type == "Puzzle" && !UserRole.HasAtLeastThePrivilegesOf(handler.Roles, UserRole.PUZZLE_EDITOR)))
            {
                return Json(new { success = false, error = "You can't handle that type of reports." });
            }
            if (!await reportRepository.HandleAsync(report.ID, judgement))
            {
                return Json(new { success = false, error = "An unexpected error happened while acknowledging the approval/rejection of the report." });
            }
            return Json(new { success = true });
        }
    }
}
