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

        public async Task MissionReviewedAsync(int missionId, int childId, string status)
        {
            await _hubContext.Clients.Group($"user-{childId}")
            .SendAsync("MissionReviewed", new { missionId, status });
        }

        public async Task MissionStartedAsync(int missionId, int childId)
        {
            await _hubContext.Clients.Group($"user-{childId}")
            .SendAsync("MissionStarted", new { missionId });
        }

        public async Task MissionSubmittedAsync(int missionId, int parentId)
        {
            await _hubContext.Clients.Group($"user-{parentId}")
            .SendAsync("MissionSubmitted", new { missionId });
        }
    }
}
