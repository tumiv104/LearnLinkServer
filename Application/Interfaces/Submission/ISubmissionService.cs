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
        Task<ApiResponse<SubmissionResponseDTO>> ApproveSubmissionAsync(ReviewSubmissionDTO submissionDto, int parentId);
        Task<ApiResponse<SubmissionResponseDTO>> RejectSubmissionAsync(ReviewSubmissionDTO submissionDto, int parentId);
		Task<ApiResponse<MissionResponse1DTO>> AcceptMissionAsync(int missionId, int childId);
		Task<ApiResponse<MissionResponse1DTO>> SubmitMissionAsync(int missionId, int childId, string fileUrl);
        Task<ApiResponse<SubmissionDetailDTO>> CheckDetailSubmissionForParents(int submissionId, int parentId);
        Task<ApiResponse<SubmissionDetailDTO>> CheckDetailSubmissionForChildren(int submissionId, int childId);
        Task<ApiResponse<PageResultDTO<SubmissionDetailDTO>>> GetAllSubmissionsForParents(int parentId, int page, int pageSize);
        Task<ApiResponse<PageResultDTO<SubmissionDetailDTO>>> GetAllSubmissionsForChildren(int childId, int page, int pageSize);
	}
}
