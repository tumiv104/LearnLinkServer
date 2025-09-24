using Application.DTOs.Common;
using Application.DTOs.Submission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Submission
{
    public interface ISubmissionService
    {
        Task<ApiResponse<SubmissionResponseDTO>> ApproveSubmissionAsync(int submissionId, int parentId);
        Task<ApiResponse<SubmissionResponseDTO>> RejectSubmissionAsync(int submissionId, int parentId, string? feedback = null);
    }
}
