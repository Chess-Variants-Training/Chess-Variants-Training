using ChessVariantsTraining.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface INotificationRepository
    {
        /*bool Add(Notification notification);
        bool Exists(string id);
        void MarkAllRead(int user);
        long UnreadCount(int user);
        List<Notification> GetNotificationsFor(int user);*/

        Task<bool> AddAsync(Notification notification);
        Task<bool> ExistsAsync(string id);
        Task MarkAllReadAsync(int user);
        Task<long> UnreadCountAsync(int user);
        Task<List<Notification>> GetNotificationsForAsync(int user);
    }
}
