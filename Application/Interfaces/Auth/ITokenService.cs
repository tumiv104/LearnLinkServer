using Domain.Entities;

namespace Application.Interfaces.Auth
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        RefreshToken GenerateRefreshToken(int userId);
    }
}
