using Application.DTOs.Auth;
using Application.DTOs.Common;
using Application.Interfaces.Auth;
using Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IAuthResponse _authResponse;

        public AuthController(IAuthResponse authResponse)
        {
            _authResponse = authResponse;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDTO userRegisterDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var success = await _authResponse.RegisterUserAsync(userRegisterDTO);
            if (!success) return BadRequestResponse("Register failed");
            return OkResponse<object>(null, "register successful");
        }

        [HttpPost("register-child")]
        public async Task<IActionResult> RegisterChild(ChildRegisterDTO childRegisterDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var success = await _authResponse.RegisterChildAsync(childRegisterDTO);
            if (!success) return BadRequestResponse("Register failed");
            return OkResponse<object>(null, "register successful");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDTO userLoginDTO)
        {
            if (!ModelState.IsValid) return BadRequestResponse("Invalid input");
            var authResponse = await _authResponse.AuthenticateUserAsync(userLoginDTO);
            if (authResponse == null) return UnauthorizedResponse();
            return OkResponse(authResponse, "Login successful");
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = HttpContext.Request.Cookies["refreshToken"];
            var authResponse = await _authResponse.RefreshTokenAsync(refreshToken);
            if (authResponse == null) return UnauthorizedResponse();
            return OkResponse(authResponse, "Refresh successful");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = HttpContext.Request.Cookies["refreshToken"];
            var success = await _authResponse.RevokeRefreshTokenAsync(refreshToken);
            if (!success) return BadRequestResponse("Logout fail");
            return OkResponse<object>(null, "Logout successful");
        }
    }
}
