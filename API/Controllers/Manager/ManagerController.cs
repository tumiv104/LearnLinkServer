using Application.DTOs.Manager;
using Application.Interfaces.Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Manager
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ManagerController : BaseController
    {
        private readonly IManagerService _manager;

        public ManagerController(IManagerService manager)
        {
            _manager = manager;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var res = await _manager.GetOverviewDtoAsync();
            if (res == null) return NotFoundResponse();
            return OkResponse(res);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsersOverview()
        {
            var res = await _manager.GetUserOverviewDtoAsync();
            if (res == null) return NotFoundResponse();
            return OkResponse(res);
        }

        [HttpGet("parents/performance")]
        public async Task<IActionResult> GetParentPerformance()
        {
            var res = await _manager.GetParentPerformanceDtoAsync();
            if (res == null) return NotFoundResponse();
            return OkResponse(res);
        }

        [HttpGet("children/performance")]
        public async Task<IActionResult> GetChildPerformance()
        {
            var res = await _manager.GetChildPerformanceDtoAsync();
            if (res == null) return NotFoundResponse();
            return OkResponse(res);
        }

        [HttpGet("completion-trend")]
        public async Task<IActionResult> GetCompletionTrend()
        {
            var res = await _manager.GetCompletionTrend();
            if (res == null) return NotFoundResponse();
            return OkResponse(res);
        }

        [HttpGet("weekly-activity")]
        public async Task<IActionResult> GetWeeklyActivity()
        {
            var res = await _manager.GetWeeklyActivity();
            if (res == null) return NotFoundResponse();
            return OkResponse(res);
        }
    }
}
