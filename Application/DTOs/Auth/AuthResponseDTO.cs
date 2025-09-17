namespace Application.DTOs.Auth
{
    public class AuthResponseDTO
    {
        public string AccessToken { get; set; } = null!;
        public DateTime AccessTokenExpiresAt { get; set; }
    }
}
