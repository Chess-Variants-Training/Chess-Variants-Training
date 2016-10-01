using ChessVariantsTraining.Models;
using Microsoft.AspNetCore.Http;

namespace ChessVariantsTraining.Services
{
    public interface IPersistentLoginHandler
    {
        int? LoggedInUserId(HttpContext context);

        User LoggedInUser(HttpContext context);

        void RegisterLogin(int user, HttpContext context);

        void Logout(HttpContext context);

        void LogoutEverywhereExceptHere(HttpContext context);

        void LogoutEverywhere(int userId);
    }
}
