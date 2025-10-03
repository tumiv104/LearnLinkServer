using Application.DTOs.Report;
using Application.Interfaces.Report;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Report
{
    public class ReportService : IReportService
    {
        private readonly LearnLinkDbContext _context;

        public ReportService(LearnLinkDbContext context)
        {
            _context = context;
        }

        public async Task<ChildProgressReportDTO> GetChildProgressAsync(int parentId, int childId, string period = "all")
        {
            var child = await _context.Users
                .Include(u => u.ChildRelations)
                .FirstOrDefaultAsync(u => u.userId == childId && u.RoleId == (int)RoleEnum.Child);

            if (child == null) return null;

            var isChildOfParent = await _context.ParentChildren
                .AnyAsync(pc => pc.ChildId == childId && pc.ParentId == parentId);
            if (!isChildOfParent) return null;
            var now = DateTime.UtcNow; 
            IQueryable<Domain.Entities.Mission> missionsQuery = _context.Missions.Where(m => m.ChildId == childId);

            switch (period.ToLower())
            {
                case "day":
                case "today":
                    missionsQuery = missionsQuery.Where(m => m.CreatedAt.Date == now.Date);
                    break;
                case "month":
                    missionsQuery = missionsQuery.Where(m => m.CreatedAt.Year == now.Year && m.CreatedAt.Month == now.Month);
                    break;
                case "year":
                    missionsQuery = missionsQuery.Where(m => m.CreatedAt.Year == now.Year);
                    break;
                case "all":
                default:
                    break;
            }

            var totalMissions = await missionsQuery.CountAsync();
            var completedMissions = await missionsQuery.CountAsync(m => m.Status == MissionStatus.Completed);
            var submittedMissions = await missionsQuery.CountAsync(m => m.Status == MissionStatus.Submitted);
            var processingMissions = await missionsQuery.CountAsync(m => m.Status == MissionStatus.Processing);
            var assignedMissions = await missionsQuery.CountAsync(m => m.Status == MissionStatus.Assigned);

            var totalPointsEarned = await missionsQuery
                .Where(m => m.Status == MissionStatus.Completed)
                .SumAsync(m => (int?)m.Points) ?? 0;

            var lastSubmissionAt = await _context.Submissions
                .Where(s => s.ChildId == childId)
                .OrderByDescending(s => s.SubmittedAt)
                .Select(s => (DateTime?)s.SubmittedAt)
                .FirstOrDefaultAsync();

            return new ChildProgressReportDTO
            {
                ChildId = childId,
                ChildName = child.Name ?? "",
                TotalMissions = totalMissions,
                CompletedMissions = completedMissions,
                SubmittedMissions = submittedMissions,
                ProcessingMissions = processingMissions,
                AssignedMissions = assignedMissions,
                TotalPointsEarned = totalPointsEarned,
                LastSubmissionAt = lastSubmissionAt
            };
        }

    }
}
