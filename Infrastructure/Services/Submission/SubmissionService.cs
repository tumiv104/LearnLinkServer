using Application.DTOs.Common;
using Application.DTOs.Mission;
using Application.DTOs.Submission;
using Application.Interfaces.Submission;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Submissions
{
	public class SubmissionService : ISubmissionService
	{
		private readonly LearnLinkDbContext _context;

		public SubmissionService(LearnLinkDbContext context)
		{
			_context = context;
		}

		public async Task<ApiResponse<SubmissionResponseDTO>> ApproveSubmissionAsync(ReviewSubmissionDTO submissionDto, int parentId) { 
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var submission = await _context.Submissions
					.Include(s => s.Mission)
					.ThenInclude(m => m.Child)
					.FirstOrDefaultAsync(s => s.SubmissionId == submissionDto.SubmissionId);

				if (submission == null)
					return new ApiResponse<SubmissionResponseDTO>(false, "Submission not found.");

				if (submission.Mission.ParentId != parentId)
					return new ApiResponse<SubmissionResponseDTO>(false, "You do not have permission to approve this submission.");

				if (submission.Status != SubmissionStatus.Pending)
					return new ApiResponse<SubmissionResponseDTO>(false, "Submission must be Pending to approve.");

				submission.Status = SubmissionStatus.Approved;
				submission.Feedback = submissionDto.Feedback;
				submission.Score = submissionDto.Score;
				submission.ReviewedAt = DateTime.UtcNow;

				submission.Mission.Status = MissionStatus.Completed;
				submission.Mission.UpdatedAt = DateTime.UtcNow;

				var childPoint = await _context.Points.FirstOrDefaultAsync(p => p.UserId == submission.ChildId);
				if (childPoint == null)
				{
					childPoint = new Point { UserId = submission.ChildId, Balance = 0 };
					_context.Points.Add(childPoint);
				}
				childPoint.Balance += submission.Mission.Points;
				childPoint.UpdatedAt = DateTime.UtcNow;

				var parentPoint = await _context.Points.FirstOrDefaultAsync(p => p.UserId == submission.Mission.ParentId);
				if (parentPoint == null)
				{
					parentPoint = new Point { UserId = submission.Mission.ParentId, Balance = 0 };
					_context.Points.Add(parentPoint);
				}

				if (parentPoint.Balance < submission.Mission.Points)
				{
					return new ApiResponse<SubmissionResponseDTO>(false, "Parent does not have enough points to approve this submission.");
				}

				parentPoint.Balance -= submission.Mission.Points;
				parentPoint.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return new ApiResponse<SubmissionResponseDTO>(true, "Submission approved successfully.",
					MapToDTO(submission));
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return new ApiResponse<SubmissionResponseDTO>(false, $"Error approving submission: {ex.Message}");
			}
		}


		public async Task<ApiResponse<SubmissionResponseDTO>> RejectSubmissionAsync(ReviewSubmissionDTO submissionDto, int parentId)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var submission = await _context.Submissions
					.Include(s => s.Mission)
					.ThenInclude(m => m.Child)
					.FirstOrDefaultAsync(s => s.SubmissionId == submissionDto.SubmissionId
					);

				if (submission == null)
					return new ApiResponse<SubmissionResponseDTO>(false, "Submission not found.");

				if (submission.Mission.ParentId != parentId)
					return new ApiResponse<SubmissionResponseDTO>(false, "You do not have permission to reject this submission.");

				if (submission.Status != SubmissionStatus.Pending)
					return new ApiResponse<SubmissionResponseDTO>(false, "Submission must be Pending to reject.");

				submission.Status = SubmissionStatus.Rejected;
				submission.Feedback = submissionDto.Feedback;
				submission.Score = submissionDto.Score;
				submission.ReviewedAt = DateTime.UtcNow;

				// mission quay lại trạng thái Processing để trẻ có thể nộp lại
				submission.Mission.Status = MissionStatus.Processing;
				submission.Mission.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return new ApiResponse<SubmissionResponseDTO>(true, "Submission rejected successfully.",
					MapToDTO(submission));
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return new ApiResponse<SubmissionResponseDTO>(false, $"Error rejecting submission: {ex.Message}");
			}
		}

		private SubmissionResponseDTO MapToDTO(Submission submission)
		{
			return new SubmissionResponseDTO
			{
				SubmissionId = submission.SubmissionId,
				MissionId = submission.MissionId,
				ChildId = submission.ChildId,
				FileUrl = submission.FileUrl,
				SubmittedAt = submission.SubmittedAt,
				Status = submission.Status.ToString(),
				Feedback = submission.Feedback,
				Score = submission.Score,
				ReviewedAt = submission.ReviewedAt
			};
		}

		public async Task<ApiResponse<MissionResponse1DTO>> AcceptMissionAsync(int missionId, int childId)
		{
			try
			{
				using var transaction = await _context.Database.BeginTransactionAsync();
				try
				{
					var child = await _context.Users
						.FirstOrDefaultAsync(u => u.userId == childId && u.RoleId == (int)RoleEnum.Child);
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
		public async Task<ApiResponse<MissionResponse1DTO>> SubmitMissionAsync(int missionId, int childId, string fileUrl)
		{
			if (string.IsNullOrEmpty(fileUrl))
			{
				return new ApiResponse<MissionResponse1DTO>(false, "Trẻ bắt buộc phải gửi ảnh để hoàn thành nhiệm vụ.");
			}

			try
			{
				using var transaction = await _context.Database.BeginTransactionAsync();

				var child = await _context.Users
					.FirstOrDefaultAsync(u => u.userId == childId && u.RoleId == (int)RoleEnum.Child);

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

				mission.AttachmentUrl = fileUrl;
				mission.Status = MissionStatus.Submitted;
				mission.UpdatedAt = DateTime.UtcNow;

				var submission = new Submission
				{
					MissionId = missionId,
					ChildId = child.userId,
					FileUrl = fileUrl,
					SubmittedAt = DateTime.UtcNow,
					Status = SubmissionStatus.Pending,
					Feedback = ""
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

		public async Task<ApiResponse<SubmissionDetailDTO>> CheckDetailSubmissionForParents(int submissionId, int parentId)
		{
			var submission = await _context.Submissions
				.Include(s => s.Mission)
				.ThenInclude(m => m.Child)
				.FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

			if (submission == null)
			{
				return new ApiResponse<SubmissionDetailDTO>(false, "Submission not found.");
			}

			bool isParentOfChild = await _context.Missions
									.AnyAsync(m => m.MissionId == submission.MissionId
									&& m.ParentId == parentId
									&& m.ChildId == submission.ChildId);

			if (!isParentOfChild)
				return new ApiResponse<SubmissionDetailDTO>(false, "Access denied");

			var dto = new SubmissionDetailDTO
			{
				SubmissionId = submission.SubmissionId,
				MissionId = submission.MissionId,
				Title = submission.Mission.Title,
				Description = submission.Mission.Description,
				Points = submission.Mission.Points,
				Deadline = submission.Mission.Deadline,
				ChildId = submission.ChildId,
				ChildName = submission.Child.Name,
				FileUrl = submission.FileUrl,
				SubmittedAt = submission.SubmittedAt,
				Status = submission.Status.ToString(),
				Feedback = submission.Feedback,
				Score = submission.Score,
				ReviewedAt = submission.ReviewedAt
			};

			return new ApiResponse<SubmissionDetailDTO>(true, "Access accepted", dto);
		}

		public async Task<ApiResponse<SubmissionDetailDTO>> CheckDetailSubmissionForChildren(int submissionId, int childId)
		{
			var submission = await _context.Submissions
				.Include(s => s.Mission)
				.ThenInclude(m => m.Parent)
				.FirstOrDefaultAsync(s => s.SubmissionId == submissionId && s.ChildId == childId);
			if (submission == null)
			{
				return new ApiResponse<SubmissionDetailDTO>(false, "Submission not found.");
			}
			var dto = new SubmissionDetailDTO
			{
				SubmissionId = submission.SubmissionId,
				MissionId = submission.MissionId,
				Title = submission.Mission.Title,
				Description = submission.Mission.Description,
				Points = submission.Mission.Points,
				Deadline = submission.Mission.Deadline,
				ChildId = submission.ChildId,
				FileUrl = submission.FileUrl,
				SubmittedAt = submission.SubmittedAt,
				Status = submission.Status.ToString(),
				Feedback = submission.Feedback,
				Score = submission.Score,
				ReviewedAt = submission.ReviewedAt
			};
			return new ApiResponse<SubmissionDetailDTO>(true, "Access accepted", dto);
		}

		public async Task<ApiResponse<PageResultDTO<SubmissionDetailDTO>>> GetAllSubmissionsForParents(int parentId, int page = 1, int pageSize = 5)
		{
			int pageNumber = page < 1 ? 1 : page;
			int[] allowedPageSize = { 5, 10, 15, 20, 30, 50 };
			int pageSizeNumber = allowedPageSize.Contains(pageSize) ? pageSize : 5;
			var query = _context.Submissions
						.Include(s => s.Mission)
						.Include(s => s.Child)
						.Where(s => s.Mission.ParentId == parentId)
						.OrderByDescending(s => s.SubmittedAt);

			int totalCount = await query.CountAsync();

			var submissions = await query
				.Skip((pageNumber - 1) * pageSizeNumber)
				.Take(pageSizeNumber)
				.Select(s => new SubmissionDetailDTO
				{
					SubmissionId = s.SubmissionId,
					MissionId = s.MissionId,
					Title = s.Mission.Title,
					Description = s.Mission.Description,
					Points = s.Mission.Points,
					Deadline = s.Mission.Deadline,
					ChildId = s.ChildId,
					ChildName = s.Child.Name,
					FileUrl = s.FileUrl,
					SubmittedAt = s.SubmittedAt,
					Status = s.Status.ToString(),
					Feedback = s.Feedback,
					Score = s.Score,
					ReviewedAt = s.ReviewedAt
				}).ToListAsync();

			return new ApiResponse<PageResultDTO<SubmissionDetailDTO>>(true, "Submissions retrieved successfully",
				new PageResultDTO<SubmissionDetailDTO>
				{
					Items = submissions,
					TotalCount = totalCount,
					PageNumber = pageNumber,
					PageSize = pageSizeNumber
				});
		}

		public async Task<ApiResponse<PageResultDTO<SubmissionDetailDTO>>> GetAllSubmissionsForChildren(int childId, int page = 1, int pageSize = 5)
		{
			int pageNumber = page < 1 ? 1 : page;
			int[] allowedPageSize = { 5, 10, 15, 20, 30, 50 };
			int pageSizeNumber = allowedPageSize.Contains(pageSize) ? pageSize : 5;
			var query = _context.Submissions
						.Include(s => s.Mission)
						.Where(s => s.ChildId == childId)
						.OrderByDescending(s => s.SubmittedAt);
			int totalCount = await query.CountAsync();
			var submissions = await query
				.Skip((pageNumber - 1) * pageSizeNumber)
				.Take(pageSizeNumber)
				.Select(s => new SubmissionDetailDTO
				{
					SubmissionId = s.SubmissionId,
					MissionId = s.MissionId,
					Title = s.Mission.Title,
					Description = s.Mission.Description,
					Points = s.Mission.Points,
					Deadline = s.Mission.Deadline,
					ChildId = s.ChildId,
					FileUrl = s.FileUrl,
					SubmittedAt = s.SubmittedAt,
					Status = s.Status.ToString(),
					Feedback = s.Feedback,
					Score = s.Score,
					ReviewedAt = s.ReviewedAt
				}).ToListAsync();
			return new ApiResponse<PageResultDTO<SubmissionDetailDTO>>(true, "Submissions retrieved successfully",
				new PageResultDTO<SubmissionDetailDTO>
				{
					Items = submissions,
					TotalCount = totalCount,
					PageNumber = pageNumber,
					PageSize = pageSizeNumber
				});
		}
	}
}
