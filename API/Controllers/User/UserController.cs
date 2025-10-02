using Application.DTOs.User;
using Application.Interfaces.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        [Authorize] 
        public async Task<IActionResult> GetProfile()
        {   
            var userIdClaim = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(userIdClaim))
                return UnauthorizedResponse();

            var userId = int.Parse(userIdClaim);

            var profile = await _userService.GetUserProfileAsync(userId);
            if (profile == null)
                return NotFoundResponse("User not found");

            return OkResponse(profile, "User profile");
        }
    }
}
