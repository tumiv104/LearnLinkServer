using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Email;
using Domain.Entities;
using Infrastructure.Data;
using Application.Interfaces.Auth;

namespace Infrastructure.Services.Auth
{
    public class PasswordService : IPasswordService
    {
        private readonly LearnLinkDbContext _context;
        private readonly IEmailService _emailService;

        public PasswordService(LearnLinkDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task RequestPasswordResetAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return;

            var token = Guid.NewGuid().ToString();
            var resetToken = new PasswordResetToken
            {
                UserId = user.userId,
                Token = token,
                ExpiredAt = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            var resetLink = $"http://localhost:3000/auth/reset-password?token={token}";
            await _emailService.SendPasswordResetEmailAsync(email, resetLink, user.Name ?? user.Email);
        }


        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var reset = await _context.PasswordResetTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == token && !x.IsUsed);

            if (reset == null || reset.ExpiredAt < DateTime.UtcNow)
                return false;

            reset.User.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            reset.IsUsed = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
                return false;

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
