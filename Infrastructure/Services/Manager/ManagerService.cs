using Application.DTOs.Manager;
using Application.Interfaces.Manager;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Manager
{
    public class ManagerService : IManagerService
    {
        private readonly LearnLinkDbContext _context;

        public ManagerService(LearnLinkDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardOverviewDto> GetOverviewDtoAsync()
        {
            // Count paren & child
            var totalParents = await _context.Users
                .Include(u => u.Role)
                .CountAsync(u => u.Role.Name == "Parent");

            var totalChildren = await _context.Users
                .Include(u => u.Role)
                .CountAsync(u => u.Role.Name == "Child");

            // Mission
            var totalMissions = await _context.Missions.CountAsync();
            var pendingMissions = await _context.Missions.CountAsync(m => m.Status == MissionStatus.Processing || m.Status == MissionStatus.Assigned);
            var completedMissions = await _context.Missions.CountAsync(m => m.Status == MissionStatus.Completed || m.Status == MissionStatus.Submitted);

            // Submission
            var totalSubmissions = await _context.Submissions.CountAsync();
            var pendingSubmissions = await _context.Submissions.CountAsync(s => s.Status == SubmissionStatus.Pending);
            var approvedSubmissions = await _context.Submissions.CountAsync(s => s.Status == SubmissionStatus.Approved);
            var rejectedSubmissions = await _context.Submissions.CountAsync(s => s.Status == SubmissionStatus.Rejected);

            // Completion rate (%)
            double completionRate = totalMissions == 0
                ? 0
                : (double)completedMissions / totalMissions * 100;

            // Today
            var today = DateTime.UtcNow.Date;
            var todayTasksAssigned = await _context.Missions
                .CountAsync(m => m.CreatedAt.Date == today);

            var todayTasksSubmitted = await _context.Submissions
                .CountAsync(s => s.SubmittedAt.Date == today);

            var overview = new DashboardOverviewDto
            {
                TotalParents = totalParents,
                TotalChildren = totalChildren,
                TotalMissions = totalMissions,
                PendingMissions = pendingMissions,
                CompletedMissions = completedMissions,
                TotalSubmissions = totalSubmissions,
                PendingSubmissions = pendingSubmissions,
                ApprovedSubmissions = approvedSubmissions,
                RejectedSubmissions = rejectedSubmissions,
                CompletionRate = Math.Round(completionRate, 2),
                TodayTasksAssigned = todayTasksAssigned,
                TodayTasksSubmitted = todayTasksSubmitted
            };

            return overview;
        }

        public async Task<IEnumerable<ParentPerformanceDto>> GetParentPerformanceDtoAsync()
        {
            var parents = await _context.Users.Include(u => u.Role)
                .Where(u => u.Role.Name == "Parent")
                .AsNoTracking()
                .ToListAsync();

            var parentMissions = await _context.Missions
                .GroupBy(m => m.ParentId)
                .Select(g => new
                {
                    ParentId = g.Key,
                    AssignedCount = g.Count(),
                    CompletedCount = g.Count(m => m.Status == MissionStatus.Completed)
                })
                .ToDictionaryAsync(g => g.ParentId);

            var result = parents.Select(p =>
            {
                parentMissions.TryGetValue(p.userId, out var stats);

                double completionRate = 0;
                if (stats != null && stats.AssignedCount > 0)
                {
                    completionRate = Math.Round((double)stats.CompletedCount / stats.AssignedCount * 100, 2);
                }

                return new ParentPerformanceDto
                {
                    ParentId = p.userId,
                    ParentName = p.Name,
                    AssignedMissions = stats?.AssignedCount ?? 0,
                    CompletedMissions = stats?.CompletedCount ?? 0,
                    CompletionRate = completionRate
                };
            }).ToList();

            return result;
        }

        public async Task<IEnumerable<ChildPerformanceDto>> GetChildPerformanceDtoAsync()
        {
            var children = await _context.Users.Include(u => u.Role)
                .Where(u => u.Role.Name == "Child")
                .AsNoTracking()
                .ToListAsync();

            // Lấy thống kê nhiệm vụ của con
            var childMissions = await _context.Missions
                .GroupBy(m => m.ChildId)
                .Select(g => new
                {
                    ChildId = g.Key,
                    AssignedCount = g.Count(),
                    CompletedCount = g.Count(m => m.Status == MissionStatus.Completed)
                })
                .ToDictionaryAsync(g => g.ChildId);

            // Lấy thống kê submission của con
            var childSubmissions = await _context.Submissions
                .GroupBy(s => s.Mission.ChildId)
                .Select(g => new
                {
                    ChildId = g.Key,
                    ValidCount = g.Count(s => s.Status == SubmissionStatus.Approved),
                    RejectedCount = g.Count(s => s.Status == SubmissionStatus.Rejected)
                })
                .ToDictionaryAsync(g => g.ChildId);

            var result = children.Select(c =>
            {
                childMissions.TryGetValue(c.userId, out var missionStats);
                childSubmissions.TryGetValue(c.userId, out var subStats);

                double completionRate = 0;
                if (missionStats != null && missionStats.AssignedCount > 0)
                {
                    completionRate = Math.Round((double)missionStats.CompletedCount / missionStats.AssignedCount * 100, 2);
                }

                return new ChildPerformanceDto
                {
                    ChildId = c.userId,
                    ChildName = c.Name,
                    AssignedMissions = missionStats?.AssignedCount ?? 0,
                    CompletedMissions = missionStats?.CompletedCount ?? 0,
                    ValidSubmissions = subStats?.ValidCount ?? 0,
                    RejectedSubmissions = subStats?.RejectedCount ?? 0,
                    CompletionRate = completionRate
                };
            }).ToList();
            return result;
        }

        public async Task<IEnumerable<UserOverviewDto>> GetUserOverviewDtoAsync()
        {
            var users = await _context.Users.Include(u => u.Role)
                .AsNoTracking()
                .Select(u => new
                {
                    u.userId,
                    u.Name,
                    u.Email,
                    Role = u.Role.Name,
                    u.CreatedAt
                })
                .ToListAsync();

            var missionStats = await _context.Missions
                .GroupBy(m => m.ParentId)
                .Select(g => new
                {
                    ParentId = g.Key,
                    AssignedCount = g.Count(),
                    CompletedCount = g.Count(m => m.Status == MissionStatus.Completed)
                })
                .ToListAsync();

            var points = await _context.Points
                .GroupBy(p => p.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Balance = g.Sum(x => x.Balance)
                })
                .ToListAsync();

            var result = users
                .Select(u =>
                {
                    var mission = missionStats.FirstOrDefault(x => x.ParentId == u.userId);
                    var userPoint = points.FirstOrDefault(p => p.UserId == u.userId);

                    return new UserOverviewDto
                    {
                        UserId = u.userId,
                        Name = u.Name,
                        Email = u.Email,
                        Role = u.Role,
                        MissionsAssigned = mission?.AssignedCount ?? 0,
                        MissionsCompleted = mission?.CompletedCount ?? 0,
                        Points = userPoint?.Balance ?? 0,
                        CreatedAt = u.CreatedAt
                    };
                })
                .ToList();
            return result;
        }

        public async Task<IEnumerable<CompletionChartDto>> GetCompletionTrend()
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-28);

            var rawData = await _context.Missions
            .Where(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate)
            .GroupBy(m => EF.Functions.DateDiffWeek(startDate, m.CreatedAt))
            .Select(g => new
            {
                WeekIndex = g.Key,
                Completed = g.Count(m => m.Status == MissionStatus.Completed),
                Total = g.Count()
            })
            .OrderBy(x => x.WeekIndex)
            .ToListAsync();

            var result = rawData
                .Select(x => new CompletionChartDto
                {
                    Week = $"Week {x.WeekIndex + 1}",
                    Rate = x.Total == 0 ? 0 : x.Completed * 100.0 / x.Total
                })
                .ToList();
            return result;
        }

        public async Task<IEnumerable<ActivityChartDto>> GetWeeklyActivity()
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-6); // 7 day
            var endDate = DateTime.UtcNow.Date.AddDays(1);

            var missions = await _context.Missions
            .Where(m => m.CreatedAt >= startDate && m.CreatedAt < endDate)
            .Select(m => new
            {
                m.CreatedAt,
                m.Status
            })
            .ToListAsync(); 

            var grouped = missions
                .GroupBy(m => m.CreatedAt.DayOfWeek)
                .Select(g => new ActivityChartDto
                {
                    Day = g.Key.ToString().Substring(0, 3), // Mon, Tue, ...
                    Assigned = g.Count(),
                    Submitted = g.Count(m => m.Status == MissionStatus.Submitted || m.Status == MissionStatus.Completed)
                })
                .ToList();

            var ordered = grouped.OrderBy(a => a.Day switch
            {
                "Mon" => 1,
                "Tue" => 2,
                "Wed" => 3,
                "Thu" => 4,
                "Fri" => 5,
                "Sat" => 6,
                "Sun" => 7,
                _ => 8
            }).ToList();

            return ordered;
        }
    }
}
