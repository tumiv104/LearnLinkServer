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
        Task<PageResultDTO<MissionDetailDTO>> ParentGetMissionsAsync(int parentId, int page = 1, int pageSize = 5);
        Task<PageResultDTO<MissionDetailDTO>> ChildGetMissionsAsync(int childId, int page = 1, int pageSize = 5);

        Task<MissionDetailDTO?> ParentGetMissionDetailAsync(int parentId, int missionId);
        Task<MissionDetailDTO?> ChildGetMissionDetailAsync(int childId, int missionId);

    }
}
