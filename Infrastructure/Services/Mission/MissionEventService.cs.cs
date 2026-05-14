using Application.Interfaces.Mission;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services.Mission
{
    public class MissionEventService : IMissionEventService
    {
        private readonly IHubContext<MissionHub> _hubContext;

        public MissionEventService(IHubContext<MissionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task MissionCreatedAsync(Domain.Entities.Mission mission)
        {
            await _hubContext.Clients.Group($"user-{mission.ChildId}")
            .SendAsync("MissionCreated", new
            {
                mission.MissionId,
                mission.Title,
                mission.Points,
                mission.Deadline
            });
        }

        public async Task MissionReviewedAsync(Domain.Entities.Submission submission, string status)
        {
            await _hubContext.Clients.Group($"user-{submission.ChildId}")
            .SendAsync("MissionReviewed", new
            {
                submission.MissionId,
                submission.Mission.Title,
                submission.Mission.Points,
                submission.Mission.Deadline,
                submission.ChildId,
                submission?.Score,
                submission?.Feedback,
                status
            });
        }

        public async Task MissionStartedAsync(Domain.Entities.Mission mission, int parentId, string childName)
        {
            await _hubContext.Clients.Group($"user-{parentId}")
            .SendAsync("MissionStarted", new
            {
                mission.MissionId,
                mission.Title,
                mission.Points,
                mission.Deadline,
                parentId,
                childName
            });
        }

        public async Task MissionSubmittedAsync(Domain.Entities.Mission mission, int parentId, string childName)
        {
            await _hubContext.Clients.Group($"user-{parentId}")
            .SendAsync("MissionSubmitted", new
            {
                mission.MissionId,
                mission.Title,
                mission.Points,
                mission.Deadline,
                parentId,
                childName
            });
        }
    }
}
