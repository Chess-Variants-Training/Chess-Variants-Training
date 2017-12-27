using ChessVariantsTraining.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ChessVariantsTraining.Services
{
    public interface IPersistentLoginHandler
    {
        User LoggedInUser(HttpContext context);
        Task<int?> LoggedInUserIdAsync(HttpContext context);
        Task<User> LoggedInUserAsync(HttpContext context);
        Task RegisterLoginAsync(int user, HttpContext context);
        Task LogoutAsync(HttpContext context);
        Task LogoutEverywhereExceptHereAsync(HttpContext context);
        Task LogoutEverywhereAsync(int userId);
    }
}
