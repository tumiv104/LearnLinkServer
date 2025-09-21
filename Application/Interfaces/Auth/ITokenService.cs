using Domain.Entities;

namespace Application.Interfaces.Auth
{
    public interface ITokenService
    {
        string GenerateAccessToken(Domain.Entities.User user);
        RefreshToken GenerateRefreshToken(int userId);
    }
}
