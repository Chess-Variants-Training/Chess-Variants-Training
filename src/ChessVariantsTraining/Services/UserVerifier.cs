using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using System;

namespace ChessVariantsTraining.Services
{
    public class UserVerifier : IUserVerifier
    {
        Settings settings;
        IUserRepository userRepository;
        IEmailSender emailSender;

        public UserVerifier(IOptions<Settings> _settings, IUserRepository _userRepository, IEmailSender _emailSender)
        {
            settings = _settings.Value;
            userRepository = _userRepository;
            emailSender = _emailSender;
        }

        public void SendVerificationEmailTo(int userId)
        {
            User user = userRepository.FindById(userId);
            emailSender.Send(user.Email, user.Username, "Chess Variants Training: email verification",
                "Use this code to verify your Chess Variants Training account: " + user.VerificationCode + Environment.NewLine + "If you didn't register for this account, please reply to this email.");
        }

        public bool Verify(int userId, int verificationCode)
        {
            User user = userRepository.FindById(userId);

            if (user.VerificationCode == verificationCode)
            {
                user.VerificationCode = 0;
                user.Verified = true;
                userRepository.Update(user);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
