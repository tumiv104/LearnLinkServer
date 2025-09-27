using Application.Interfaces.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers.Report
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : BaseController
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("child-progress/{childId}")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> GetChildProgress(int childId)
        {
            var parentId = int.Parse(User.FindFirstValue("id"));
            var report = await _reportService.GetChildProgressAsync(parentId, childId);

            if (report == null)
                return NotFoundResponse("Không tìm thấy hoặc không phải con của bạn");

            return OkResponse(report, "Báo cáo tiến độ của con");
        }
    }
}


