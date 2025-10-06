using Application.DTOs.Points;
using Application.Interfaces.Points;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [HttpPost("redeem/product")]
        public async Task<IActionResult> RedeemProduct(RedeemProductDTO dto)
        {
            var res = await _pointService.ChildRedeemProduct(dto);
            if (res == false) return BadRequestResponse("");
            return OkResponse(res);
        }

        [HttpGet("redemption/child")]
        [Authorize(Roles = "Child")]
        public async Task<IActionResult> GetAllRedemptionChild()
        {
            var childIdClaim = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(childIdClaim))
                return UnauthorizedResponse();

            var childId = int.Parse(childIdClaim);
            var res = await _pointService.GetAllRedemptionsByChildAsync(childId);
            return OkResponse(res);
        }

        [HttpGet("redemption/getAll")]
        public async Task<IActionResult> GetAllRedemption()
        {
            var res = await _pointService.GetAllRedemptionsAsync();
         
            return OkResponse(res);
        }

        [HttpPost("redemption/updateStatus")]
        public async Task<IActionResult> UpdateStatus(UpdateStatusRedemptionDTO dto)
        {
            var res = await _pointService.UpdateRedemptionStatus(dto.RedemptionId, dto.NewStatus);
            if (res == null) return BadRequestResponse("");
            return OkResponse(res);
        }
    }
}
