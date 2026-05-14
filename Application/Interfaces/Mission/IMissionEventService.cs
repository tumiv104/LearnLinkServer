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
        Task MissionStartedAsync(Domain.Entities.Mission mission, int parentId, string childName);
        Task MissionSubmittedAsync(Domain.Entities.Mission mission, int parentId, string childName);
        Task MissionReviewedAsync(Domain.Entities.Submission submission, string status);
    }
}
