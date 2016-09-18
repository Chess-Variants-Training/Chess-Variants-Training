using ChessVariantsTraining.Attributes;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Models;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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
        public IActionResult Index()
        {
            int loggedIn = loginHandler.LoggedInUserId(HttpContext).Value;
            List<Notification> notifications = notificationRepository.GetNotificationsFor(loggedIn);
            notificationRepository.MarkAllRead(loggedIn);
            return View(notifications);
        }
    }
}
