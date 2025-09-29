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

        public async Task<ApiResponse<SubmissionResponseDTO>> ApproveSubmissionAsync(int submissionId, int parentId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var submission = await _context.Submissions
                    .Include(s => s.Mission)
                    .ThenInclude(m => m.Child)
                    .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

                if (submission == null)
                    return new ApiResponse<SubmissionResponseDTO>(false, "Submission not found.");

                if (submission.Mission.ParentId != parentId)
                    return new ApiResponse<SubmissionResponseDTO>(false, "You do not have permission to approve this submission.");

                if (submission.Status != SubmissionStatus.Pending)
                    return new ApiResponse<SubmissionResponseDTO>(false, "Submission must be Pending to approve.");

                submission.Status = SubmissionStatus.Approved;
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


        public async Task<ApiResponse<SubmissionResponseDTO>> RejectSubmissionAsync(int submissionId, int parentId, string? feedback = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var submission = await _context.Submissions
                    .Include(s => s.Mission)
                    .ThenInclude(m => m.Child)
                    .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

                if (submission == null)
                    return new ApiResponse<SubmissionResponseDTO>(false, "Submission not found.");

                if (submission.Mission.ParentId != parentId)
                    return new ApiResponse<SubmissionResponseDTO>(false, "You do not have permission to reject this submission.");

                if (submission.Status != SubmissionStatus.Pending)
                    return new ApiResponse<SubmissionResponseDTO>(false, "Submission must be Pending to reject.");

                submission.Status = SubmissionStatus.Rejected;
                submission.Feedback = feedback ?? submission.Feedback;
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
		public async Task<ApiResponse<MissionResponse1DTO>> SubmitWithImageAsync(int missionId, int childId, string attachmentUrl, string feedback)
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
	}
}
