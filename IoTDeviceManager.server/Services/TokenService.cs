using IoTDeviceManager.server.Data;
using IoTDeviceManager.server.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IoTDeviceManager.server.Services
{
    public class TokenService(UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        public async Task<TokenResponse> GenerateTokensAsync(ApplicationUser user)
        {
            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.UniqueName, user.UserName!),
                new("email", user.Email!),
                new Claim(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            ];

            var i = 1;
            foreach (var role in (await userManager.GetRolesAsync(user)).ToList())
            {
                claims.Add(new($"role{i}", role));
                i++;
            }

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);
            DateTime expires = DateTime.Now.AddMinutes(Convert.ToDouble(configuration["Jwt:ExpireMinutes"]));

            JwtSecurityToken token = new(
                configuration["Jwt:Issuer"],
                configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = GenerateRefreshToken();

            RefreshToken? existingToken = await context.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (existingToken is null)
            {
                RefreshToken tokenEntity = new()
                {
                    Id = Guid.CreateVersion7().ToString(),
                    Token = refreshToken,
                    UserId = user.Id,
                    Expires = DateTimeOffset.Now.AddDays(7),
                    CreatedAt = DateTimeOffset.Now
                };

                await context.RefreshTokens.AddAsync(tokenEntity);
                var addedCount = await context.SaveChangesAsync();
            }
            else
            {
                existingToken.Token = refreshToken;
                existingToken.Expires = DateTimeOffset.Now.AddDays(7);
                existingToken.CreatedAt = DateTimeOffset.Now;

                context.RefreshTokens.Update(existingToken);

                var updatedCount = await context.SaveChangesAsync();
            }

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            return Convert.ToBase64String(randomBytes);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                ValidateLifetime = false
            };

            JwtSecurityTokenHandler tokenHandler = new();

            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            return principal;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            RefreshToken? storedRefreshToken = await context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.Token == refreshToken);

            return storedRefreshToken is not null && storedRefreshToken.Expires >= DateTime.UtcNow && !storedRefreshToken.Revoked;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken) => await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
    }
}
