using Application.DTOs.User;
using Application.Interfaces.User;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.User
{
    public class UserService : IUserService
    {
        private readonly LearnLinkDbContext _context;

        public UserService(LearnLinkDbContext context)
        {
            _context = context;
        }
        public async Task<UserProfileDTO?> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.ParentRelations)
                    .ThenInclude(pc => pc.Child)
                .Include(u => u.ChildRelations)
                    .ThenInclude(pc => pc.Parent)
                .FirstOrDefaultAsync(u => u.userId == userId);

            if (user == null)
                return null;

            var totalPoints = await _context.Points
                .Where(p => p.UserId == userId)
                .SumAsync(p => (int?)p.Balance) ?? 0;

            var childrenCount = user.RoleId == (int)RoleEnum.Parent
                ? user.ParentRelations?.Count ?? 0
                : 0;

            string? parentName = null;
            if (user.RoleId == (int)RoleEnum.Child)
            {
                parentName = user.ChildRelations?
                    .Select(pc => pc.Parent?.Name)
                    .FirstOrDefault();
            }

            return new UserProfileDTO
            {
                UserId = user.userId,
                Name = user.Name,
                Email = user.Email,
                RoleName = user.Role?.Name ?? string.Empty,
                Dob = user.Dob,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt,
                TotalPoints = totalPoints,
                ChildrenCount = childrenCount,
                ParentName = parentName
            };
        }
        public async Task<bool> UpdateUserProfileAsync(int userId, UserProfileUpdateDTO updateDTO)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.userId == userId);
            if (user == null) return false;

            if (!string.IsNullOrWhiteSpace(updateDTO.Name))
                user.Name = updateDTO.Name;

            if (updateDTO.Dob.HasValue && updateDTO.Dob.Value < DateTime.UtcNow.Date)
                user.Dob = updateDTO.Dob;

            if (!string.IsNullOrEmpty(updateDTO.AvatarUrl))
                user.AvatarUrl = updateDTO.AvatarUrl;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

    }
}
