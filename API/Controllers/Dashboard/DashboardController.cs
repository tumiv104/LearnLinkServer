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

        [HttpGet("parent-overview/{parentId}")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> GetParentOverview(int parentId)
        {
            var result = await _dashboardService.GetParentOverviewAsync(parentId);
            if (result == null)
                return NotFoundResponse("No data found for this parent");

            return OkResponse(result, "Parent dashboard overview");
        }
    }
}
