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





        // Parent xem danh sách nhiệm vụ của các con mình
        public async Task<PageResultDTO<MissionResponseDTO>> ParentGetMissionsAsync(int parentId, int page = 1, int pageSize = 5)
        {
            var parent = await _context.Users
                .Include(u => u.ParentRelations)
                .ThenInclude(pc => pc.Child)
                .FirstOrDefaultAsync(u => u.userId == parentId && u.RoleId == (int)RoleEnum.Parent);

            int pageNumber = page < 1 ? 1 : page;
            int pageSizeNumber = pageSize < 1 ? 5 : pageSize;

            if (parent == null)
                return new PageResultDTO<MissionResponseDTO>
                {
                    Items = new List<MissionResponseDTO>(),
                    PageNumber = pageNumber,
                    PageSize = pageSizeNumber,
                    TotalPages = 0,
                    TotalCount = 0
                };

            var childIds = parent.ParentRelations.Select(pc => pc.ChildId).ToList();
            if (!childIds.Any())
                return new PageResultDTO<MissionResponseDTO>
                {
                    Items = new List<MissionResponseDTO>(),
                    PageNumber = pageNumber,
                    PageSize = pageSizeNumber,
                    TotalPages = 0,
                    TotalCount = 0
                };

            var query = _context.Missions
                .Where(m => childIds.Contains(m.ChildId))
                .OrderByDescending(m => m.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSizeNumber);

            var items = await query
                .Skip((pageNumber - 1) * pageSizeNumber)
                .Take(pageSizeNumber)
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

            int serialStart = (pageNumber - 1) * pageSizeNumber;
            for (int i = 0; i < items.Count; i++)
            {
                items[i].SerialNumber = serialStart + i + 1;
            }

            return new PageResultDTO<MissionResponseDTO>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSizeNumber,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }


        // Child xem danh sách nhiệm vụ của mình
        public async Task<PageResultDTO<MissionResponseDTO>> ChildGetMissionsAsync(int childId, int page = 1, int pageSize = 5)
        {
            var child = await _context.Users
                .FirstOrDefaultAsync(u => u.userId == childId && u.RoleId == (int)RoleEnum.Child);

            int pageNumber = page < 1 ? 1 : page;
            int pageSizeNumber = pageSize < 1 ? 5 : pageSize;

            if (child == null)
                return new PageResultDTO<MissionResponseDTO>
                {
                    Items = new List<MissionResponseDTO>(),
                    PageNumber = pageNumber,
                    PageSize = pageSizeNumber,
                    TotalPages = 0,
                    TotalCount = 0
                };

            var query = _context.Missions
                .Where(m => m.ChildId == childId)
                .OrderByDescending(m => m.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSizeNumber);

            var items = await query
                .Skip((pageNumber - 1) * pageSizeNumber)
                .Take(pageSizeNumber)
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

            int serialStart = (pageNumber - 1) * pageSizeNumber;
            for (int i = 0; i < items.Count; i++)
            {
                items[i].SerialNumber = serialStart + i + 1;
            }

            return new PageResultDTO<MissionResponseDTO>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSizeNumber,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }

        public async Task<ApiResponse<MissionResponse1DTO>> AcceptMissionAsync(int missionId, string childEmail)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var child = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == childEmail && u.RoleId == (int)RoleEnum.Child);
                    if (child == null)
                    {
                        await transaction.RollbackAsync();
                        return new ApiResponse<MissionResponse1DTO>(false, "Không tìm thấy trẻ với email này.");
                    }

                    var mission = await _context.Missions
                        .Include(m => m.Parent)
                        .Include(m => m.Child)
                        .FirstOrDefaultAsync(m => m.MissionId == missionId && m.ChildId == child.userId);

                    if (mission == null)
                    {
                        await transaction.RollbackAsync();
                        return new ApiResponse<MissionResponse1DTO>(false, "Nhiệm vụ không tồn tại hoặc không được giao cho trẻ này.");
                    }

                    if (mission.Status != MissionStatus.Assigned)
                    {
                        await transaction.RollbackAsync();
                        return new ApiResponse<MissionResponse1DTO>(false, "Nhiệm vụ không ở trạng thái Assigned để chấp nhận.");
                    }

                    mission.Status = MissionStatus.Processing;
                    mission.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var response = MapToResponseDTO(mission);
                    return new ApiResponse<MissionResponse1DTO>(true, "Nhiệm vụ đã được chấp nhận thành công.", response);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<MissionResponse1DTO>(false, $"Lỗi khi chấp nhận nhiệm vụ: {ex.Message}");
            }
        }

        public async Task<ApiResponse<MissionResponse1DTO>> SubmitWithImageAsync(
    int missionId,
    string childEmail,
    string attachmentUrl,
    string feedback)
        {
            if (string.IsNullOrEmpty(attachmentUrl))
            {
                return new ApiResponse<MissionResponse1DTO>(false, "Trẻ bắt buộc phải gửi ảnh để hoàn thành nhiệm vụ.");
            }

            if (string.IsNullOrWhiteSpace(feedback))
            {
                return new ApiResponse<MissionResponse1DTO>(false, "Feedback không được rỗng.");
            }

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var child = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == childEmail && u.RoleId == (int)RoleEnum.Child);

                if (child == null)
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<MissionResponse1DTO>(false, "Không tìm thấy trẻ với email này.");
                }

                var mission = await _context.Missions
                    .Include(m => m.Parent)
                    .Include(m => m.Child)
                    .FirstOrDefaultAsync(m => m.MissionId == missionId);

                if (mission == null)
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<MissionResponse1DTO>(false, "Nhiệm vụ không tồn tại.");
                }

                // kiểm tra xem nhiệm vụ có thuộc về đứa trẻ này không
                if (mission.ChildId != child.userId)
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<MissionResponse1DTO>(false, "Nhiệm vụ này không được giao cho trẻ này.");
                }

                if (mission.Status != MissionStatus.Processing)
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<MissionResponse1DTO>(false, "Nhiệm vụ phải ở trạng thái Processing để nộp.");
                }

                mission.AttachmentUrl = attachmentUrl;
                mission.Status = MissionStatus.Submitted;
                mission.UpdatedAt = DateTime.UtcNow;

                var submission = new Submission
                {
                    MissionId = missionId,
                    ChildId = child.userId,
                    FileUrl = attachmentUrl,
                    SubmittedAt = DateTime.UtcNow,
                    Status = SubmissionStatus.Pending,
                    Feedback = feedback
                };

                _context.Submissions.Add(submission);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = MapToResponseDTO(mission);
                return new ApiResponse<MissionResponse1DTO>(true, "Nhiệm vụ đã được nộp thành công với ảnh.", response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<MissionResponse1DTO>(false, $"Lỗi khi nộp nhiệm vụ: {ex.Message} | {ex.InnerException?.Message}");
            }
        }



        private static MissionResponse1DTO MapToResponseDTO(Domain.Entities.Mission mission)
        {
            if (mission == null) return null;

            return new MissionResponse1DTO
            {
                MissionId = mission.MissionId,
                ChildId = mission.ChildId,
                Title = mission.Title,
                Description = mission.Description,
                Points = mission.Points,
                Deadline = mission.Deadline,
                Status = mission.Status,
                CreatedAt = mission.CreatedAt,
                AttachmentUrl = mission.AttachmentUrl,
                ImageUrl = mission.AttachmentUrl,

            };
        }

        public async Task<ApiResponse<MissionResponse1DTO>> GetMissionByIdAsync(int missionId, string childEmail)
        {
            try
            {
                var child = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == childEmail && u.RoleId == (int)RoleEnum.Child);
                if (child == null)
                {
                    return new ApiResponse<MissionResponse1DTO>(false, "Không tìm thấy trẻ với email này.");
                }

                var mission = await _context.Missions
                    .Include(m => m.Parent)
                    .Include(m => m.Child)
                    .Include(m => m.Submissions)
                    .FirstOrDefaultAsync(m => m.MissionId == missionId && m.ChildId == child.userId);

                if (mission == null)
                {
                    return new ApiResponse<MissionResponse1DTO>(false, "Nhiệm vụ không tồn tại hoặc không được giao cho trẻ này.");
                }

                var response = MapToResponseDTO(mission);
                return new ApiResponse<MissionResponse1DTO>(true, "Nhiệm vụ được lấy thành công.", response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<MissionResponse1DTO>(false, $"Lỗi khi lấy nhiệm vụ: {ex.Message}");
            }
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
    }


    }

