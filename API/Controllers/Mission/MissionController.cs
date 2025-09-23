using Application.DTOs.Mission;
using Application.Interfaces.Common;
using Application.Interfaces.Missions;
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
        private readonly IFileStorage _fileStorage;
        private readonly IWebHostEnvironment _env;

        public MissionController(IMissionService missionService,
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

            // lấy email parent
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            if (string.IsNullOrEmpty(email)) return UnauthorizedResponse();

            // nếu có file thì upload
            if (attachmentFile != null)
            {
                using var stream = attachmentFile.OpenReadStream();
                var url = await _fileStorage.SaveAsync(stream, attachmentFile.FileName, "missions", _env.WebRootPath);
                dto.AttachmentUrl = url;
            }

            var result = await _missionService.AssignMissionAsync(email, dto);
            if (!result.Success)
                return BadRequestResponse(result.Message);

            return OkResponse<object>(null, result.Message);
        }




        // Parent xem danh sách nhiệm vụ của các con mình (có phân trang)
        [HttpGet("parent-missions")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> GetParentMissions(int page = 1)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            if (string.IsNullOrEmpty(email)) return UnauthorizedResponse();

            var missions = await _missionService.ParentGetMissionsAsync(email, page, 5);
            return OkResponse(missions, "List of missions for your children");
        }

        // Child xem danh sách nhiệm vụ của mình (có phân trang)
        [HttpGet("child-missions")]
        [Authorize(Roles = "Child")]
        public async Task<IActionResult> GetChildMissions(int page = 1)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            if (string.IsNullOrEmpty(email)) return UnauthorizedResponse();

            var missions = await _missionService.ChildGetMissionsAsync(email, page, 5);
            return OkResponse(missions, "List of your missions");
        }

        // Parent xem chi tiết nhiệm vụ cụ thể của con mình
        [HttpGet("parent-mission/{id}")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> GetParentMissionDetail(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            if (string.IsNullOrEmpty(email)) return UnauthorizedResponse();

            var mission = await _missionService.ParentGetMissionDetailAsync(email, id);
            if (mission == null)
                return NotFoundResponse("Mission not found or you do not have permission");

            return OkResponse(mission, "Mission detail");
        }

        // Child xem chi tiết nhiệm vụ cụ thể của mình
        [HttpGet("child-mission/{id}")]
        [Authorize(Roles = "Child")]
        public async Task<IActionResult> GetChildMissionDetail(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            if (string.IsNullOrEmpty(email)) return UnauthorizedResponse();

            var mission = await _missionService.ChildGetMissionDetailAsync(email, id);
            if (mission == null)
                return NotFoundResponse("Mission not found or you do not have permission");

            return OkResponse(mission, "Mission detail");
        }

    }
}
