using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Common;
using Application.DTOs.Mission;

namespace Application.Interfaces.Missions
{
    public interface IMissionService
    {
        Task<AssignMissionResult> AssignMissionAsync(string parentEmail, MissionAssignDTO dto);
        Task<PageResultDTO<MissionResponseDTO>> ParentGetMissionsAsync(string parentEmail, int page = 1, int pageSize = 5);
        Task<PageResultDTO<MissionResponseDTO>> ChildGetMissionsAsync(string childEmail, int page = 1, int pageSize = 5);
    }
}
