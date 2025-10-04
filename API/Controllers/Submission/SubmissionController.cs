using Application.DTOs.Mission;
using Application.DTOs.Submission;
using Application.Interfaces.Common;
using Application.Interfaces.Submission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers.Submission
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubmissionController : BaseController
    {
        private readonly ISubmissionService _submissionService;
		private readonly IFileStorage _fileStorage;
		private readonly IWebHostEnvironment _env;

		public SubmissionController(ISubmissionService submissionService,
			IFileStorage fileStorage,
			IWebHostEnvironment env)
        {
            _submissionService = submissionService;
            _fileStorage = fileStorage;
            _env = env;
		}

        // Phụ huynh duyệt submission
        [HttpPost("approve")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> ApproveSubmission(ReviewSubmissionDTO submissionDto)
        {
            var parentIdClaim = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(parentIdClaim)) return UnauthorizedResponse();

            int parentId = int.Parse(parentIdClaim);

            var result = await _submissionService.ApproveSubmissionAsync(submissionDto, parentId);
            if (!result.Success)
                return BadRequestResponse(result.Message);

            return OkResponse(result.Data, result.Message);
        }

        // Phụ huynh từ chối submission
        [HttpPost("reject")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> RejectSubmission(ReviewSubmissionDTO dto)
        {
            var parentIdClaim = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(parentIdClaim)) return UnauthorizedResponse();

            int parentId = int.Parse(parentIdClaim);

            var result = await _submissionService.RejectSubmissionAsync(dto, parentId);
            if (!result.Success)
                return BadRequestResponse(result.Message);

            return OkResponse(result.Data, result.Message);
        }

		// Child chấp nhận nhiệm vụ
		[HttpPost("accept")]
		[Authorize(Roles = "Child")]
		public async Task<IActionResult> AcceptMission([FromBody] AcceptMissionDTO dto)
		{
			if (!ModelState.IsValid)
				return BadRequestResponse("Invalid data");

			var childIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");
			if (string.IsNullOrEmpty(childIdClaim)) return UnauthorizedResponse();

			var childId = int.Parse(childIdClaim);

			var result = await _submissionService.AcceptMissionAsync(dto.MissionId, childId);
			if (!result.Success)
				return BadRequestResponse(result.Message);

			return OkResponse(result.Data, result.Message);
		}

		// Child nộp nhiệm vụ kèm ảnh
		[HttpPost("missions/{missionId}/submit")]
		[Authorize(Roles = "Child")]
		public async Task<IActionResult> SubmitMission(int missionId, IFormFile file)
		{
			var childIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");
			if (string.IsNullOrEmpty(childIdClaim))
				return Unauthorized(new { message = "Không tìm thấy id trong token" });

			var childId = int.Parse(childIdClaim);
			if (file == null || file.Length == 0)
				return BadRequest(new { message = "Trẻ bắt buộc phải gửi ảnh để nộp nhiệm vụ." });

			string fileUrl;
			using (var stream = file.OpenReadStream())
			{
                fileUrl = await _fileStorage.SaveAsync(stream, file.FileName, "submissions", _env.WebRootPath);
			}

			var result = await _submissionService.SubmitMissionAsync(
				missionId,
				childId,
                fileUrl
            );

			return Ok(result);
		}

		// Phụ huynh xem chi tiết submission
		[HttpGet("{submissionId}/details/parents")]
		[Authorize(Roles = "Parent")]
		public async Task<IActionResult> GetSubmissionDetailsForParents(int submissionId)
		{
			var parentIdClaim = User.FindFirstValue("id");
			if (string.IsNullOrEmpty(parentIdClaim)) return UnauthorizedResponse();
			int parentId = int.Parse(parentIdClaim);
			var result = await _submissionService.CheckDetailSubmissionForParents(submissionId, parentId);
			if (!result.Success)
				return BadRequestResponse(result.Message);
			return OkResponse(result.Data, result.Message);
		}
		// Trẻ xem chi tiết submission
		[HttpGet("{submissionId}/details/children")]
		[Authorize(Roles = "Child")]
		public async Task<IActionResult> GetSubmissionDetailsForChildren(int submissionId)
		{
			var childIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");
			if (string.IsNullOrEmpty(childIdClaim)) return UnauthorizedResponse();
			int childId = int.Parse(childIdClaim);
			var result = await _submissionService.CheckDetailSubmissionForChildren(submissionId, childId);
			if (!result.Success)
				return BadRequestResponse(result.Message);
			return OkResponse(result.Data, result.Message);
		}


        // Phụ huynh lấy tất cả submission của họ
        [HttpGet("parents")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> GetAllSubmissionsForParents([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var parentIdClaim = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(parentIdClaim)) return UnauthorizedResponse();
            int parentId = int.Parse(parentIdClaim);

            var result = await _submissionService.GetAllSubmissionsForParents(parentId, page, pageSize);
            if (!result.Success)
                return BadRequestResponse(result.Message);

            return OkResponse(result.Data, result.Message);
        }


        // Trẻ lấy tất cả submission của chúng
        [HttpGet("children{childid}")]
		[Authorize (Roles = "Child")]
		public async Task<IActionResult> GetAllSubmissionsForChildren(int childid, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
		{
			var childIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");
			if (string.IsNullOrEmpty(childIdClaim)) return UnauthorizedResponse();
			int childId = int.Parse(childIdClaim);
			var result = await _submissionService.GetAllSubmissionsForChildren(childId, page, pageSize);
			if (!result.Success)
				return BadRequestResponse(result.Message);
			return OkResponse(result.Data, result.Message);
		}
	}
}
