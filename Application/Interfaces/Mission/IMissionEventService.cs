using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Mission
{
    public interface IMissionEventService
    {
        Task MissionCreatedAsync(Domain.Entities.Mission mission);
        Task MissionStartedAsync(int missionId, int childId);
        Task MissionSubmittedAsync(int missionId, int parentId);
        Task MissionReviewedAsync(int missionId, int childId, string status);
    }
}
