using Application.Interfaces.Points;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Points
{
    [Route("api/[controller]")]
    [ApiController]
    public class PointController : BaseController
    {
        private readonly IPointService _pointService;

        public PointController(IPointService pointService)
        {
            _pointService = pointService;
        }

        [HttpGet("detail/{userId}")]
        public async Task<IActionResult> GetPointDetailByUserId(int userId)
        {
            var pointRes = await _pointService.GetPointByUserId(userId);
            if (pointRes == null) return NotFound();
            return OkResponse(pointRes);
        }
    }
}
