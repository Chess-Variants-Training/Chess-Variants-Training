using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Models;
using Microsoft.AspNet.Http;
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
            int? idFromSession = context.Session.GetInt32("userId");
            if (idFromSession.HasValue)
            {
                return idFromSession;
            }

            IReadableStringCollection requestCookies = context.Request.Cookies;
            if (!requestCookies.ContainsKey("login"))
            {
                return null;
            }

            string[] cookieParts = requestCookies["login"].First().Split(':');
            long identifier;
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
            using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
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
                login = new SavedLogin(user);
            } while (savedLoginRepository.ContainsID(login.ID));
            savedLoginRepository.Add(login);
            context.Response.Cookies.Append("login", login.ID + ":" + login.UnhashedToken);

            context.Session.SetInt32("userId", user);
        }

        public void Logout(HttpContext context)
        {
            IReadableStringCollection requestCookies = context.Request.Cookies;
            if (!requestCookies.ContainsKey("login"))
            {
                return;
            }

            string[] cookieParts = requestCookies["login"].First().Split(':');

            long identifier;
            if (!long.TryParse(cookieParts[0], out identifier))
            {
                return;
            }

            savedLoginRepository.Delete(identifier);
            context.Response.Cookies.Delete("login", new CookieOptions() { HttpOnly = true });

            context.Session.Remove("userId");
            context.Session.Remove("abc");
        }
    }
}
