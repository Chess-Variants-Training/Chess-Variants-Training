using ChessVariantsTraining.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface INotificationRepository
    {
        Task<bool> AddAsync(Notification notification);
        Task<bool> ExistsAsync(string id);
        Task MarkAllReadAsync(int user);
        Task<long> UnreadCountAsync(int user);
        Task<List<Notification>> GetNotificationsForAsync(int user);
    }
}
