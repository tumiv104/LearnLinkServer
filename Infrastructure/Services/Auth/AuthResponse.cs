using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Auth
{
    public class AuthResponse : IAuthResponse
    {
        private readonly LearnLinkDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public AuthResponse(LearnLinkDbContext context, ITokenService tokenService, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _tokenService = tokenService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResponseDTO?> AuthenticateUserAsync(UserLoginDTO userLoginDTO)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == userLoginDTO.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDTO.Password, user.Password)) return null;


            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user.userId);
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = refreshToken.Expires
            });

            var accessTokenExpiryMinutes = int.TryParse(_configuration["JwtSettings:AccessTokenExpiryMinutes"], out var val) ? val : 15;
            return new AuthResponseDTO
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes)
            };
        }

        public async Task<AuthResponseDTO?> RefreshTokenAsync(string refreshToken)
        {
            var existingToken = await _context.RefreshTokens.Include(rt => rt.User).ThenInclude(u => u.Role).FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (existingToken == null || existingToken.IsRevoked || existingToken.Expires <= DateTime.UtcNow) return null;
            existingToken.IsRevoked = true;
            existingToken.RevokedAt = DateTime.UtcNow;

            var user = existingToken.User;
            var accessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken(user.userId);
            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", newRefreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = newRefreshToken.Expires
            });

            var accessTokenExpiryMinutes = int.TryParse(_configuration["JwtSettings:AccessTokenExpiryMinutes"], out var val) ? val : 15;
            return new AuthResponseDTO
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes)
            };
        }

        public async Task<bool> RegisterChildAsync(ChildRegisterDTO childRegisterDTO)
        {
            if (await _context.Users.AnyAsync(u => u.Email == childRegisterDTO.Email)) return false;
            var child = new Domain.Entities.User
            {
                Name = childRegisterDTO.Name,
                Email = childRegisterDTO.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(childRegisterDTO.Password),
                Dob = childRegisterDTO.Dob,
                AvatarUrl = childRegisterDTO.AvatarUrl,
                RoleId = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _context.Users.Add(child);
            await _context.SaveChangesAsync();

            var relation = new ParentChild
            {
                ParentId = childRegisterDTO.ParentId,
                ChildId = child.userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ParentChildren.Add(relation);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RegisterUserAsync(UserRegisterDTO userRegisterDTO)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userRegisterDTO.Email)) return false;
            var user = new Domain.Entities.User
            {
                Name = userRegisterDTO.Name,
                Email = userRegisterDTO.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(userRegisterDTO.Password),
                Dob = userRegisterDTO.Dob,
                AvatarUrl = userRegisterDTO.AvatarUrl,
                RoleId = userRegisterDTO.RoleId ?? 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var existingToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (existingToken == null || existingToken.IsRevoked) return false;
            existingToken.IsRevoked = true;
            existingToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("refreshToken");
            return true;
        }
    }
}
