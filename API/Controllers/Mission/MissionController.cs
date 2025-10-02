using Application.DTOs.Mission;
using Application.Interfaces.Common;
using Application.Interfaces.Missions;
using Application.Interfaces.Submission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers.Mission
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionController : BaseController
    {
        private readonly IMissionService _missionService;
        private readonly ISubmissionService _submissionService;
		private readonly IFileStorage _fileStorage;
        private readonly IWebHostEnvironment _env;


        public MissionController(
            IMissionService missionService,
            IFileStorage fileStorage,
            IWebHostEnvironment env)
        {
            _missionService = missionService;
            _fileStorage = fileStorage;
            _env = env;
        }

        // Parent giao nhiệm vụ cho con
        [HttpPost("assign")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> AssignMission([FromForm] MissionAssignDTO dto, IFormFile? attachmentFile)
        {
            if (!ModelState.IsValid)
                return BadRequestResponse("Invalid data");
            var parentIdClaim = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(parentIdClaim))
                return UnauthorizedResponse();

            var parentId = int.Parse(parentIdClaim);

            if (attachmentFile != null)
            {
                using var stream = attachmentFile.OpenReadStream();
                var url = await _fileStorage.SaveAsync(stream, attachmentFile.FileName, "missions", _env.WebRootPath);
                dto.AttachmentUrl = url;
            }

            var result = await _missionService.AssignMissionAsync(parentId, dto);
            if (!result.Success)
                return BadRequestResponse(result.Message);

            return OkResponse<object>(null, result.Message);
        }


        // Parent xem danh sách nhiệm vụ của các con mình (có phân trang)
        [HttpGet("parent-missions")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> GetParentMissions(int page = 1, int pageSize = 5)
        {
            var parentIdClaim = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(parentIdClaim))
                return UnauthorizedResponse();

            var parentId = int.Parse(parentIdClaim);

            var missions = await _missionService.ParentGetMissionsAsync(parentId, page, pageSize);
            return OkResponse(missions, "List of missions for your children");
        }

        // Child xem danh sách nhiệm vụ của mình (có phân trang)
        [HttpGet("child-missions")]
        [Authorize(Roles = "Child")]
        public async Task<IActionResult> GetChildMissions(int page = 1, int pageSize = 5)
        {
            var childIdClaim = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(childIdClaim))
                return UnauthorizedResponse();

            var childId = int.Parse(childIdClaim);

            var missions = await _missionService.ChildGetMissionsAsync(childId, page, pageSize);
            return OkResponse(missions, "List of your missions");
        }

        // Parent xem chi tiết nhiệm vụ cụ thể của con mình
        [HttpGet("parent-mission/{id}")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> GetParentMissionDetail(int id)
        {
            var parentIdClaim = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(parentIdClaim)) return UnauthorizedResponse();

            var parentId = int.Parse(parentIdClaim);

            var mission = await _missionService.ParentGetMissionDetailAsync(parentId, id);
            if (mission == null)
                return NotFoundResponse("Mission not found or you do not have permission");

            return OkResponse(mission, "Mission detail");
        }

        // Child xem chi tiết nhiệm vụ cụ thể của mình
        [HttpGet("child-mission/{id}")]
        [Authorize(Roles = "Child")]
        public async Task<IActionResult> GetChildMissionDetail(int id)
        {
            var childIdClaim = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(childIdClaim)) return UnauthorizedResponse();

            var childId = int.Parse(childIdClaim);

            var mission = await _missionService.ChildGetMissionDetailAsync(childId, id);
            if (mission == null)
                return NotFoundResponse("Mission not found or you do not have permission");

            return OkResponse(mission, "Mission detail");
        }

        // Child get List Mission with Submission by mission status
        [HttpGet("child/status")]
        [Authorize(Roles = "Child")]
        public async Task<IActionResult> GetChildMissionsByStatus(string status)
        {
            var childIdClaim = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(childIdClaim))
                return UnauthorizedResponse();

            var childId = int.Parse(childIdClaim);

            var missions = await _missionService.GetChildMissionByStatus(childId, status);
            return OkResponse(missions, "List of your missions");
        }
    }
}
