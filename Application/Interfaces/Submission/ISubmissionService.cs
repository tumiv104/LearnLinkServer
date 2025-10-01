using Application.DTOs.Common;
using Application.DTOs.Mission;
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
		Task<ApiResponse<MissionResponse1DTO>> AcceptMissionAsync(int missionId, int childId);
		Task<ApiResponse<MissionResponse1DTO>> SubmitWithImageAsync(int missionId, int childId, string imageUrl, string feedback);
        Task<ApiResponse<SubmissionDetailDTO>> CheckDetailSubmissionForParents(int submissionId, int parentId);
        Task<ApiResponse<SubmissionDetailDTO>> CheckDetailSubmissionForChildren(int submissionId, int childId);
	}
}
