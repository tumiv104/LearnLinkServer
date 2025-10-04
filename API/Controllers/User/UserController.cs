using Application.DTOs.User;
using Application.Interfaces.Common;
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
        private readonly IFileStorage _fileStorage;
        private readonly IWebHostEnvironment _env;

        public UserController(IUserService userService, IFileStorage fileStorage,
            IWebHostEnvironment env)
        {
            _userService = userService;
            _fileStorage = fileStorage;
            _env = env;
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

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> EditProfile([FromForm] UserProfileUpdateDTO updateDTO)
        {
            var userIdClaim = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(userIdClaim))
                return UnauthorizedResponse();

            var userId = int.Parse(userIdClaim);

            if (updateDTO.AvatarFile != null)
            {
                using var stream = updateDTO.AvatarFile.OpenReadStream();
                var url = await _fileStorage.SaveAsync(stream, updateDTO.AvatarFile.FileName, "avatars", _env.WebRootPath);
                updateDTO.AvatarUrl = url;
            }

            var success = await _userService.UpdateUserProfileAsync(userId, updateDTO);
            if (!success) return BadRequestResponse("Failed to update profile");

            return OkResponse<object>(null, "Profile updated successfully");
        }

    }
}
