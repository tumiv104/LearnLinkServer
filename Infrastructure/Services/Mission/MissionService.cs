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
using Application.Interfaces.Mission;
using Infrastructure.Services.Mission;

namespace Infrastructure.Services.Missions
{
    public class MissionService : IMissionService
    {
        private readonly LearnLinkDbContext _context;
        private readonly IMissionEventService _missionEventService;

        public MissionService(LearnLinkDbContext context, IMissionEventService missionEventService)
        {
            _context = context;
            _missionEventService = missionEventService;
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
            await _missionEventService.MissionCreatedAsync(mission);
            return new AssignMissionResult(true, "Mission assigned successfully");
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

        //public async Task<ApiResponse<MissionResponse1DTO>> GetMissionByIdAsync(int missionId, string childEmail)
        //{
        //    try
        //    {
        //        var child = await _context.Users
        //            .FirstOrDefaultAsync(u => u.Email == childEmail && u.RoleId == (int)RoleEnum.Child);
        //        if (child == null)
        //        {
        //            return new ApiResponse<MissionResponse1DTO>(false, "Không tìm thấy trẻ với email này.");
        //        }

        //        var mission = await _context.Missions
        //            .Include(m => m.Parent)
        //            .Include(m => m.Child)
        //            .Include(m => m.Submissions)
        //            .FirstOrDefaultAsync(m => m.MissionId == missionId && m.ChildId == child.userId);

        //        if (mission == null)
        //        {
        //            return new ApiResponse<MissionResponse1DTO>(false, "Nhiệm vụ không tồn tại hoặc không được giao cho trẻ này.");
        //        }

        //        var response = MapToResponseDTO(mission);
        //        return new ApiResponse<MissionResponse1DTO>(true, "Nhiệm vụ được lấy thành công.", response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ApiResponse<MissionResponse1DTO>(false, $"Lỗi khi lấy nhiệm vụ: {ex.Message}");
        //    }
        //}
    }
}

