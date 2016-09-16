using ChessVariantsTraining.Models;
using System.Collections.Generic;

namespace ChessVariantsTraining.DbRepositories
{
    public interface INotificationRepository
    {
        bool Add(Notification notification);

        bool Exists(string id);

        void MarkAllRead(int user);

        long UnreadCount(int user);

        List<Notification> GetNotificationsFor(int user);
    }
}
