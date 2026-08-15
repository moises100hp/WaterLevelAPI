using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WaterLevelAPI.Model;

namespace WaterLevelAPI.Service
{
    public static class JwtService
    {
        public static string GenerateToken(User user, IConfiguration configuration)
        {
            var jwtKey = configuration["Jwt:Key"] ?? "WaterLevelApi_DevelopmentKey_ChangeMe_1234567890";
            var issuer = configuration["Jwt:Issuer"] ?? "WaterLevelAPI";
            var audience = configuration["Jwt:Audience"] ?? "WaterLevelAPI";

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
