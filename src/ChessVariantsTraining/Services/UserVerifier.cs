using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

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

        public async Task SendVerificationEmailToAsync(int userId)
        {
            User user = await userRepository.FindByIdAsync(userId);
            emailSender.Send(user.Email, user.Username, "Chess Variants Training: email verification",
                "Use this code to verify your Chess Variants Training account: " + user.VerificationCode + Environment.NewLine + "If you didn't register for this account, please reply to this email.");
        }

        public async Task<bool> VerifyAsync(int userId, int verificationCode)
        {
            User user = await userRepository.FindByIdAsync(userId);

            if (user.VerificationCode == verificationCode)
            {
                user.VerificationCode = 0;
                user.Verified = true;
                await userRepository.UpdateAsync(user);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
