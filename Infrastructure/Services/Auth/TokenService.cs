using Application.Interfaces.Auth;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(Domain.Entities.User user)
        {
            var claims = new[]
            {
                new Claim("id", user.userId.ToString()),
                new Claim("email", user.Email),
                new Claim("role", user.Role.Name),
                new Claim("name", user.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:securitykey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:AccessTokenExpiryMinutes"]));

            var token = new JwtSecurityToken(

                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RefreshToken GenerateRefreshToken(int userId)
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var expiryDays = Convert.ToInt32(_configuration["JwtSettings:RefreshTokenExpiryDays"]);
            var expiryDate = DateTime.UtcNow.AddDays(expiryDays);

            return new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                Expires = expiryDate,
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
                RevokedAt = null
            };
        }
    }
}
