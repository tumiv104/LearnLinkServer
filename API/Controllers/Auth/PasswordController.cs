using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordController : ControllerBase
    {
        private readonly IPasswordService _passwordService;

        public PasswordController(IPasswordService passwordService)
        {
            _passwordService = passwordService;
        }

        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _passwordService.RequestPasswordResetAsync(request.Email);
            return Ok("Nếu email tồn tại, link đặt lại mật khẩu đã được gửi.");
        }


        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
                return BadRequest("Token and new password are required");

            var success = await _passwordService.ResetPasswordAsync(request.Token, request.NewPassword);
            if (!success)
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");

            return Ok("Đặt lại mật khẩu thành công!");
        }


        [HttpPost("change")]
        [Authorize] 
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User not authenticated");

            var userId = int.Parse(userIdClaim);

            var success = await _passwordService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);
            if (!success)
                return BadRequest("Mật khẩu cũ không đúng hoặc người dùng không tồn tại.");

            return Ok("Đổi mật khẩu thành công!");
        }
    }
}
