using ChessVariantsTraining.Models;
using Microsoft.AspNet.Http;

namespace ChessVariantsTraining.Services
{
    public interface IPersistentLoginHandler
    {
        int? LoggedInUserId(HttpContext context);

        User LoggedInUser(HttpContext context);

        void RegisterLogin(int user, HttpContext context);

        void Logout(HttpContext context);
    }
}
