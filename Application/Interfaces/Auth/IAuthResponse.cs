using Application.DTOs.Auth;

namespace Application.Interfaces.Auth
{
    public interface IAuthResponse
    {
        Task<bool> RegisterUserAsync(UserRegisterDTO userRegisterDTO);
        Task<AuthResponseDTO?> AuthenticateUserAsync(UserLoginDTO userLoginDTO);
        Task<AuthResponseDTO?> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    }
}
