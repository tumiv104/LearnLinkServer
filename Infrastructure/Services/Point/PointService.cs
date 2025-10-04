using Application.DTOs.Points;
using Application.Interfaces.Points;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Points
{
    public class PointService : IPointService
    {
        private readonly LearnLinkDbContext _context;

        public PointService(LearnLinkDbContext context)
        {
            _context = context;
        }

        public async Task<PointDTO> GetPointByUserId(int userId)
        {
            var point = await _context.Points.Include(p => p.User).ThenInclude(u => u.Role).FirstOrDefaultAsync(p  => p.UserId == userId);
            if (point == null) return null;
            return new PointDTO
            {
                UserId = userId,
                UserName = point.User.Name,
                UserRole = point.User.Role.Name,
                UserEmail = point.User.Email,
                Balance = point.Balance
            };
        }
    }
}
