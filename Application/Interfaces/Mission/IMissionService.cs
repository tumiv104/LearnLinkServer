using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Common;
using Application.DTOs.Mission;

namespace Application.Interfaces.Missions
{
    public interface IMissionService
    {
        Task<AssignMissionResult> AssignMissionAsync(int parentId, MissionAssignDTO dto);
        Task<PageResultDTO<MissionResponseDTO>> ParentGetMissionsAsync(int parentId, int page = 1, int pageSize = 5);
        Task<PageResultDTO<MissionResponseDTO>> ChildGetMissionsAsync(int childId, int page = 1, int pageSize = 5);

        //Task<ApiResponse<MissionResponse1DTO>> AcceptMissionAsync(int missionId, int childId);
        Task<ApiResponse<MissionResponse1DTO>> AcceptMissionAsync(int missionId, string childEmail);
        Task<ApiResponse<MissionResponse1DTO>> SubmitWithImageAsync(int missionId, string childEmail, string imageUrl, string feedback);
        Task<ApiResponse<MissionResponse1DTO>> GetMissionByIdAsync(int missionId, string childEmail);
        Task<MissionDetailDTO?> ParentGetMissionDetailAsync(int parentId, int missionId);
        Task<MissionDetailDTO?> ChildGetMissionDetailAsync(int childId, int missionId);

    }
}
