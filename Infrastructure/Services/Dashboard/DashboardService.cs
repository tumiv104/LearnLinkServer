using Application.DTOs.Dashboard;
using Application.DTOs.Report;
using Application.Interfaces.Dashboard;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly LearnLinkDbContext _context;

        public DashboardService(LearnLinkDbContext context)
        {
            _context = context;
        }

        public async Task<ParentOverviewDTO> GetParentOverviewAsync(int parentId)
        {
            var missions = _context.Missions.Where(m => m.ParentId == parentId);

            var stats = new OverviewStatsDTO
            {
                TotalMissions = await missions.CountAsync(),
                CompletedMissions = await missions.CountAsync(m => m.Status == MissionStatus.Completed),
                PendingSubmissions = await _context.Submissions
                    .CountAsync(s => s.Mission.ParentId == parentId && s.Status == SubmissionStatus.Pending),

                PendingRedemptions = await _context.Redemptions
                    .Where(r => r.Status == RedemptionStatus.Pending
                                && _context.ParentChildren.Any(pc => pc.ParentId == parentId && pc.ChildId == r.ChildId))
                    .CountAsync()
            };

            var children = await _context.ParentChildren
                .Where(pc => pc.ParentId == parentId)
                .Select(pc => new ChildSummaryDTO
                {
                    UserId = pc.Child.userId,
                    Name = pc.Child.Name,
                    AvatarUrl = pc.Child.AvatarUrl,
                    TotalPoints = pc.Child.Points != null && pc.Child.Points.Any()
                        ? pc.Child.Points.Sum(p => (int?)p.Balance) ?? 0
                        : 0
                }).ToListAsync();

            //var notifications = await _context.Notifications
            //    .Where(n => n.UserId == parentId)
            //    .OrderByDescending(n => n.CreatedAt)
            //    .Take(10)
            //    .Select(n => new NotificationDTO
            //    {
            //        NotificationId = n.NotificationId,
            //        Message = n.Message,
            //        CreatedAt = n.CreatedAt,
            //        Type = n.Type.ToString(),
            //        IsRead = n.IsRead
            //    }).ToListAsync();

            return new ParentOverviewDTO
            {
                Overview = stats,
                Children = children ?? new List<ChildSummaryDTO>(),
                RecentNotifications = new List<NotificationDTO>()
            };
        }

    }

}
