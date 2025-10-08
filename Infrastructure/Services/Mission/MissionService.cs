using Application.DTOs.Common;
using Application.DTOs.Mission;
using Application.Interfaces.Common;
using Application.Interfaces.Missions;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Missions;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Application.DTOs.Common;
using Azure;
using Application.DTOs.Submission;

namespace Infrastructure.Services.Missions
{
    public class MissionService : IMissionService
    {
        private readonly LearnLinkDbContext _context;

        public MissionService(LearnLinkDbContext context)
        {
            _context = context;
        }

        // Parent giao nhiệm vụ cho con
        public async Task<AssignMissionResult> AssignMissionAsync(int parentId, MissionAssignDTO dto)
        {
            var parent = await _context.Users
                .Include(u => u.ParentRelations)
                .FirstOrDefaultAsync(u => u.userId == parentId && u.RoleId == (int)RoleEnum.Parent);

            if (parent == null)
                return new AssignMissionResult(false, "Parent not found");

            var isChild = parent.ParentRelations.Any(pc => pc.ChildId == dto.ChildId);
            if (!isChild)
                return new AssignMissionResult(false, "Child does not belong to this Parent");

            if (dto.Deadline < DateTime.UtcNow)
                return new AssignMissionResult(false, "Deadline cannot be in the past");

            if (dto.Points < 0)
                return new AssignMissionResult(false, "Points cannot be negative");

            var parentPoint = await _context.Points.FirstOrDefaultAsync(p => p.UserId == parentId);
            if (parentPoint == null || parentPoint.Balance < dto.Points)
                return new AssignMissionResult(false, "Parent does not have enough points to assign this mission");

            var mission = new Domain.Entities.Mission
            {
                ParentId = parentId,
                ChildId = dto.ChildId,
                Title = dto.Title,
                Description = dto.Description,
                Points = dto.Points,
                Promise = dto.Promise,
                Punishment = dto.Punishment,
                Deadline = dto.Deadline,
                AttachmentUrl = dto.AttachmentUrl ?? "no-attachment",
                Status = MissionStatus.Assigned,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Missions.Add(mission);
            await _context.SaveChangesAsync();

            return new AssignMissionResult(true, "Mission assigned successfully");
        }

		public async Task<ApiResponse<MissionEditDTO>> ParentEditMission(int missionId, int parentId, MissionEditDTO dto)
		{
			var mission = await _context.Missions
				.FirstOrDefaultAsync(m => m.MissionId == missionId && m.ParentId == parentId);

			if (mission == null)
				return new ApiResponse<MissionEditDTO>(false, "Mission not found or does not belong to this Parent");

			if (mission.Status != MissionStatus.Assigned)
				return new ApiResponse<MissionEditDTO>(false, "Only missions with status 'Assigned' can be edited");

			bool isChanged = false;

			if (!string.IsNullOrEmpty(dto.Title) && dto.Title != mission.Title)
			{
				mission.Title = dto.Title;
				isChanged = true;
			}

			if (!string.IsNullOrEmpty(dto.Description) && dto.Description != mission.Description)
			{
				mission.Description = dto.Description;
				isChanged = true;
			}

			if (dto.Points.HasValue && dto.Points.Value != mission.Points)
			{
				if (dto.Points.Value < 0)
					return new ApiResponse<MissionEditDTO>(false, "Points cannot be negative");

				mission.Points = dto.Points.Value;
				isChanged = true;
			}

			if (!string.IsNullOrEmpty(dto.Promise) && dto.Promise != mission.Promise)
			{
				mission.Promise = dto.Promise;
				isChanged = true;
			}

			if (!string.IsNullOrEmpty(dto.Punishment) && dto.Punishment != mission.Punishment)
			{
				mission.Punishment = dto.Punishment;
				isChanged = true;
			}

			if (dto.Deadline.HasValue && dto.Deadline.Value != mission.Deadline)
			{
				if (dto.Deadline.Value < DateTime.UtcNow)
					return new ApiResponse<MissionEditDTO>(false, "Deadline cannot be in the past");

				mission.Deadline = dto.Deadline.Value;
				isChanged = true;
			}

			// Xử lý AttachmentUrl từ DTO (nếu controller truyền vào link mới)
			if (!string.IsNullOrEmpty(dto.AttachmentUrl) && dto.AttachmentUrl != mission.AttachmentUrl)
			{
				mission.AttachmentUrl = dto.AttachmentUrl;
				isChanged = true;
			}

			if (!isChanged)
				return new ApiResponse<MissionEditDTO>(false, "No changes detected, mission not updated");

			mission.UpdatedAt = DateTime.UtcNow;
			await _context.SaveChangesAsync();

			return new ApiResponse<MissionEditDTO>(true, "Mission updated successfully");
		}

		// Parent xem danh sách nhiệm vụ của các con mình
		public async Task<PageResultDTO<MissionDetailDTO>> ParentGetMissionsAsync(int parentId, int page = 1, int pageSize = 5)
        {
            var parent = await _context.Users
                .Include(u => u.ParentRelations)
                .FirstOrDefaultAsync(u => u.userId == parentId && u.RoleId == (int)RoleEnum.Parent);

            int pageNumber = page < 1 ? 1 : page;
            int pageSizeNumber = pageSize < 1 ? 5 : pageSize;

            if (parent == null)
                return new PageResultDTO<MissionDetailDTO>
                {
                    Items = new List<MissionDetailDTO>(),
                    PageNumber = pageNumber,
                    PageSize = pageSizeNumber,
                    TotalPages = 0,
                    TotalCount = 0
                };

            var childIds = parent.ParentRelations.Select(pc => pc.ChildId).ToList();
            if (!childIds.Any())
                return new PageResultDTO<MissionDetailDTO>
                {
                    Items = new List<MissionDetailDTO>(),
                    PageNumber = pageNumber,
                    PageSize = pageSizeNumber,
                    TotalPages = 0,
                    TotalCount = 0
                };

            var query = _context.Missions
                .Include(m => m.Child)
                .Include(m => m.Submissions)
                .Where(m => childIds.Contains(m.ChildId))
                .OrderByDescending(m => m.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSizeNumber);

            var items = await query
            .Skip((pageNumber - 1) * pageSizeNumber)
            .Take(pageSizeNumber)
            .Select(m => new MissionDetailDTO
            {
                MissionId = m.MissionId,
                Title = m.Title,
                Description = m.Description,
                Points = m.Points,
                Deadline = m.Deadline,
                Status = m.Status.ToString(),
                Promise = m.Promise,
                Punishment = m.Punishment,
                AttachmentUrl = m.AttachmentUrl,
                CreatedAt = m.CreatedAt,
                ChildId = m.ChildId,
                ChildName = m.Child.Name,
                LastSubmittedAt = m.Submissions
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => (DateTime?)s.SubmittedAt)
            .FirstOrDefault()
    })
    .ToListAsync();

            return new PageResultDTO<MissionDetailDTO>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSizeNumber,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }
        
        // Child xem danh sách nhiệm vụ của mình
        public async Task<PageResultDTO<MissionDetailDTO>> ChildGetMissionsAsync(int childId, int page = 1, int pageSize = 5)
        {
            var child = await _context.Users
                .FirstOrDefaultAsync(u => u.userId == childId && u.RoleId == (int)RoleEnum.Child);

            int pageNumber = page < 1 ? 1 : page;
            int pageSizeNumber = pageSize < 1 ? 5 : pageSize;

            if (child == null)
                return new PageResultDTO<MissionDetailDTO>
                {
                    Items = new List<MissionDetailDTO>(),
                    PageNumber = pageNumber,
                    PageSize = pageSizeNumber,
                    TotalPages = 0,
                    TotalCount = 0
                };

            var query = _context.Missions
                .Include(m => m.Submissions)
                .Where(m => m.ChildId == childId)
                .OrderByDescending(m => m.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSizeNumber);

            var items = await query
                .Skip((pageNumber - 1) * pageSizeNumber)
                .Take(pageSizeNumber)
                .Select(m => new MissionDetailDTO
                {
                    MissionId = m.MissionId,
                    Title = m.Title,
                    Description = m.Description,
                    Points = m.Points,
                    Deadline = m.Deadline,
                    Status = m.Status.ToString(),
                    Promise = m.Promise,
                    Punishment = m.Punishment,
                    AttachmentUrl = m.AttachmentUrl,
                    CreatedAt = m.CreatedAt,
                    ChildId = m.ChildId,
                    LastSubmittedAt = m.Submissions
                        .OrderByDescending(s => s.SubmittedAt)
                        .Select(s => (DateTime?)s.SubmittedAt)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return new PageResultDTO<MissionDetailDTO>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSizeNumber,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }
        
        public async Task<MissionDetailDTO?> ParentGetMissionDetailAsync(int parentId, int missionId)
        {
            var parent = await _context.Users
                .Include(u => u.ParentRelations)
                .FirstOrDefaultAsync(u => u.userId == parentId && u.RoleId == (int)RoleEnum.Parent);

            if (parent == null) return null;

            var childIds = parent.ParentRelations.Select(pc => pc.ChildId).ToList();
            if (!childIds.Any()) return null;

            var mission = await _context.Missions
                .Where(m => m.MissionId == missionId && childIds.Contains(m.ChildId))
                .FirstOrDefaultAsync();

            if (mission == null) return null;

            return new MissionDetailDTO
            {
                MissionId = mission.MissionId,
                Title = mission.Title,
                Description = mission.Description,
                Points = mission.Points,
                Deadline = mission.Deadline,
                Status = mission.Status.ToString(),
                Promise = mission.Promise,
                Punishment = mission.Punishment,
                AttachmentUrl = mission.AttachmentUrl,
                CreatedAt = mission.CreatedAt,
                ChildId = mission.ChildId
            };
        }

        public async Task<MissionDetailDTO?> ChildGetMissionDetailAsync(int childId, int missionId)
        {
            var child = await _context.Users
                .FirstOrDefaultAsync(u => u.userId == childId && u.RoleId == (int)RoleEnum.Child);

            if (child == null) return null;

            var mission = await _context.Missions
                .Where(m => m.MissionId == missionId && m.ChildId == child.userId)
                .FirstOrDefaultAsync();

            if (mission == null) return null;

            return new MissionDetailDTO
            {
                MissionId = mission.MissionId,
                Title = mission.Title,
                Description = mission.Description,
                Points = mission.Points,
                Deadline = mission.Deadline,
                Status = mission.Status.ToString(),
                Promise = mission.Promise,
                Punishment = mission.Punishment,
                AttachmentUrl = mission.AttachmentUrl,
                CreatedAt = mission.CreatedAt,
                ChildId = mission.ChildId
            };
        }

        public async Task<List<MissionWithSubmissionDTO>> GetChildMissionByStatus(int childId, string status)
        {
            var child = await _context.Users
                .FirstOrDefaultAsync(u => u.userId == childId);

            if (child == null)
                return null;

            MissionStatus? parsedStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<MissionStatus>(status, true, out var enumStatus))
            {
                parsedStatus = enumStatus;
            }

            if (parsedStatus == null && status != "All") return null;

            var query = _context.Missions
                .Include(m => m.Submissions)
                .Where(m => m.ChildId == childId && (parsedStatus == null || m.Status == parsedStatus.Value))
                .OrderByDescending(m => m.CreatedAt);

            var items = await query
                .Select(m => new MissionWithSubmissionDTO
                {
                    MissionId = m.MissionId,
                    Title = m.Title,
                    Description = m.Description,
                    Points = m.Points,
                    Deadline = m.Deadline,
                    MissionStatus = m.Status.ToString(),
                    Promise = m.Promise,
                    Punishment = m.Punishment,
                    AttachmentUrl = m.AttachmentUrl,
                    CreatedAt = m.CreatedAt,
                    Submission = m.Submissions.OrderByDescending(s => s.SubmittedAt)
                        .Select(s => new SubmissionResponseDTO
                        {
                            SubmissionId = s.SubmissionId,
                            MissionId = s.MissionId,
                            ChildId = s.ChildId,
                            FileUrl = s.FileUrl,
                            SubmittedAt = s.SubmittedAt,
                            Status = s.Status.ToString(),
                            Feedback = s.Feedback,
                            Score = s.Score,
                            ReviewedAt = s.ReviewedAt
                        }).FirstOrDefault()
                })
                .ToListAsync();

            return items;
        }

		public async Task<ApiResponse<MissionByTimeRangeDTO>> ChildGetMissionsByAllRangesAsync(int childId)
		{
			var child = await _context.Users
				.FirstOrDefaultAsync(u => u.userId == childId && u.RoleId == (int)RoleEnum.Child);

			if (child == null)
				return new ApiResponse<MissionByTimeRangeDTO>(false, "Child not found");

			DateTime now = DateTime.UtcNow;

			// Ngày
			var todayStart = now.Date;
			var todayEnd = todayStart.AddDays(1);

			// Tuần (tính từ thứ 2 -> CN)
			int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
			var weekStart = now.AddDays(-diff).Date;
			var weekEnd = weekStart.AddDays(7);

			// Tháng
			var monthStart = new DateTime(now.Year, now.Month, 1);
			var monthEnd = monthStart.AddMonths(1);

			// Query base
			var baseQuery = _context.Missions
				.Where(m => m.ChildId == childId);

			var todayMissions = await baseQuery
				.Where(m => m.CreatedAt >= todayStart && m.CreatedAt < todayEnd)
				.OrderByDescending(m => m.CreatedAt)
				.Select(m => new MissionResponseDTO
				{
					MissionId = m.MissionId,
					Title = m.Title,
					Description = m.Description,
					Points = m.Points,
					Deadline = m.Deadline,
					Status = m.Status.ToString(),
					CreatedAt = m.CreatedAt,
					ChildId = m.ChildId
				})
				.ToListAsync();

			var weekMissions = await baseQuery
				.Where(m => m.CreatedAt >= weekStart && m.CreatedAt < weekEnd)
				.OrderByDescending(m => m.CreatedAt)
				.Select(m => new MissionResponseDTO
				{
					MissionId = m.MissionId,
					Title = m.Title,
					Description = m.Description,
					Points = m.Points,
					Deadline = m.Deadline,
					Status = m.Status.ToString(),
					CreatedAt = m.CreatedAt,
					ChildId = m.ChildId
				})
				.ToListAsync();

			var monthMissions = await baseQuery
				.Where(m => m.CreatedAt >= monthStart && m.CreatedAt < monthEnd)
				.OrderByDescending(m => m.CreatedAt)
				.Select(m => new MissionResponseDTO
				{
					MissionId = m.MissionId,
					Title = m.Title,
					Description = m.Description,
					Points = m.Points,
					Deadline = m.Deadline,
					Status = m.Status.ToString(),
					CreatedAt = m.CreatedAt,
					ChildId = m.ChildId
				})
				.ToListAsync();

			var result = new MissionByTimeRangeDTO
			{
				TodayMissions = todayMissions,
				WeekMissions = weekMissions,
				MonthMissions = monthMissions
			};

			return new ApiResponse<MissionByTimeRangeDTO>(true, "Missions retrieved successfully", result);
		}


	}
}

