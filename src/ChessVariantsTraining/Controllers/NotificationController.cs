using ChessVariantsTraining.Attributes;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Models;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.Controllers
{
    [Restricted(true, UserRole.NONE)]
    public class NotificationController : CVTController
    {
        INotificationRepository notificationRepository;

        public NotificationController(IUserRepository _userRepository, IPersistentLoginHandler _loginHandler, INotificationRepository _notificationRepository)
            : base(_userRepository, _loginHandler)
        {
            notificationRepository = _notificationRepository;
        }

        [Route("/Notifications")]
        public async Task<IActionResult> Index()
        {
            int loggedIn = (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value;
            List<Notification> notifications = await notificationRepository.GetNotificationsForAsync(loggedIn);
            await notificationRepository.MarkAllReadAsync(loggedIn);
            return View(notifications);
        }
    }
}
