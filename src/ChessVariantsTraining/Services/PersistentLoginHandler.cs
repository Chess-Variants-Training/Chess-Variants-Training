using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace ChessVariantsTraining.Services
{
    public class PersistentLoginHandler : IPersistentLoginHandler
    {
        ISavedLoginRepository savedLoginRepository;
        IUserRepository userRepository;

        public PersistentLoginHandler(ISavedLoginRepository _savedLoginRepository, IUserRepository _userRepository)
        {
            savedLoginRepository = _savedLoginRepository;
            userRepository = _userRepository;
        }

        public User LoggedInUser(HttpContext context)
        {
            int? id = LoggedInUserId(context);
            if (!id.HasValue) return null;
            return userRepository.FindById(id.Value);
        }

        public int? LoggedInUserId(HttpContext context)
        {
            IRequestCookieCollection requestCookies = context.Request.Cookies;

            if (!requestCookies.ContainsKey("login"))
            {
                return null;
            }

            long identifier;
            string[] cookieParts = requestCookies["login"].Split(':');

            if (!long.TryParse(cookieParts[0], out identifier))
            {
                return null;
            }

            if (!savedLoginRepository.ContainsID(identifier))
            {
                return null;
            }

            string hex = cookieParts[1];
            byte[] unhashedToken = Enumerable.Range(0, hex.Length >> 1)
                                             .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
                                             .ToArray();

            byte[] hashedToken;
            using (SHA256 sha256 = SHA256.Create())
            {
                hashedToken = sha256.ComputeHash(unhashedToken);
            }

            return savedLoginRepository.AuthenticatedUser(identifier, hashedToken);
        }

        public void RegisterLogin(int user, HttpContext context)
        {
            SavedLogin login;
            do
            {
                login = new SavedLogin(user, context.Request.Headers["X-Forwarded-For"]);
            } while (savedLoginRepository.ContainsID(login.ID));
            savedLoginRepository.Add(login);
            context.Response.Cookies.Append("login", login.ID + ":" + login.UnhashedToken, new CookieOptions() { HttpOnly = true, Secure = true,
                Expires = new DateTimeOffset(DateTime.UtcNow.AddDays(30), new TimeSpan(0)) });
        }

        public void Logout(HttpContext context)
        {
            IRequestCookieCollection requestCookies = context.Request.Cookies;
            if (!requestCookies.ContainsKey("login"))
            {
                return;
            }

            string[] cookieParts = requestCookies["login"].Split(':');

            long identifier;
            if (!long.TryParse(cookieParts[0], out identifier))
            {
                return;
            }

            savedLoginRepository.Delete(identifier);
            context.Response.Cookies.Delete("login");
        }

        public void LogoutEverywhereExceptHere(HttpContext context)
        {
            int? loggedInUserId = LoggedInUserId(context);
            if (!loggedInUserId.HasValue)
            {
                return;
            }

            savedLoginRepository.DeleteAllOfExcept(loggedInUserId.Value, long.Parse(context.Request.Cookies["login"].Split(':')[0]));
        }

        public void LogoutEverywhere(int userId)
        {
            savedLoginRepository.DeleteAllOf(userId);
        }
    }
}
