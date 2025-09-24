using Application.DTOs.Submission;
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

        public SubmissionController(ISubmissionService submissionService)
        {
            _submissionService = submissionService;
        }

        // Phụ huynh duyệt submission
        [HttpPost("{submissionId}/approve")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> ApproveSubmission(int submissionId)
        {
            var parentIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(parentIdClaim)) return UnauthorizedResponse();

            int parentId = int.Parse(parentIdClaim);

            var result = await _submissionService.ApproveSubmissionAsync(submissionId, parentId);
            if (!result.Success)
                return BadRequestResponse(result.Message);

            return OkResponse(result.Data, result.Message);
        }

        // Phụ huynh từ chối submission
        [HttpPost("{submissionId}/reject")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> RejectSubmission(int submissionId, [FromBody] RejectSubmissionDTO dto)
        {
            var parentIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(parentIdClaim)) return UnauthorizedResponse();

            int parentId = int.Parse(parentIdClaim);

            var result = await _submissionService.RejectSubmissionAsync(submissionId, parentId, dto.Feedback);
            if (!result.Success)
                return BadRequestResponse(result.Message);

            return OkResponse(result.Data, result.Message);
        }
    }
}
