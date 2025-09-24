using Application.DTOs.Common;
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
    }
}
