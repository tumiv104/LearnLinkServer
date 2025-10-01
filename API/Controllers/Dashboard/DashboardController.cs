using Application.DTOs.Dashboard;
using Application.Interfaces.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers.Dashboard
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("parent-overview")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> GetParentOverview()
        {
            var parentIdClaim = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(parentIdClaim))
                return UnauthorizedResponse();

            var parentId = int.Parse(parentIdClaim);

            var result = await _dashboardService.GetParentOverviewAsync(parentId);
            if (result == null)
                return NotFoundResponse("No data found for this parent");

            return OkResponse(result, "Parent dashboard overview");
        }
    }
}
