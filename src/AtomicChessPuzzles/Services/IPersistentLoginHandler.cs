using AtomicChessPuzzles.Models;
using Microsoft.AspNet.Http;

namespace AtomicChessPuzzles.Services
{
    public interface IPersistentLoginHandler
    {
        int? LoggedInUserId(HttpContext context);

        User LoggedInUser(HttpContext context);

        void RegisterLogin(int user, HttpContext context);

        void Logout(HttpContext context);
    }
}
