using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blog.Api.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Blog.Api.Security;

public class JwtService (IConfiguration configuration,UserManager<User> userManagers) : IJwtService
{
     public string GenerateJwtToken(User user)
     {
          var jwtSettings = configuration.GetSection("JwtSettings");
          var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

          var claims = new List<Claim>
          {
               new Claim(ClaimTypes.NameIdentifier, user.Id),
               new Claim(ClaimTypes.Email, user.Email),
               new Claim(ClaimTypes.Name, user.UserName)
          };

          // var roles = userManagers.GetRolesAsync(user).Result;
          // foreach (var role in roles)
          // {
          //      claims.Add(new Claim(ClaimTypes.Role, role));
          // }

          var tokenDescriptor = new SecurityTokenDescriptor
          {
               Subject = new ClaimsIdentity(claims),
               Expires = DateTime.UtcNow.AddHours(1),
               Issuer = jwtSettings["Issuer"],
               Audience = jwtSettings["Audience"],
               SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
          };

          var tokenHandler = new JwtSecurityTokenHandler();
          var token = tokenHandler.CreateToken(tokenDescriptor);

          return tokenHandler.WriteToken(token);
     }

     public string GenerateRefreshToken()
     {
          var randomNumber = new byte[32];
          using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
          rng.GetBytes(randomNumber);
          return Convert.ToBase64String(randomNumber);
     }

     public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
     {
          var jwtSettings = configuration.GetSection("JwtSettings");
          var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

          var tokenValidationParameters = new TokenValidationParameters
          {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = false, // Do not validate lifetime for expired tokens
               ValidateIssuerSigningKey = true,
               ValidIssuer = jwtSettings["Issuer"],
               ValidAudience = jwtSettings["Audience"],
               IssuerSigningKey = new SymmetricSecurityKey(key)
          };

          var tokenHandler = new JwtSecurityTokenHandler();
          try
          {
               var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
               if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
               {
                    throw new SecurityTokenException("Invalid token.");
               }

               return principal;
          }
          catch
          {
               return null;
          }
     }
}

public interface IJwtService
{
     string GenerateJwtToken(User user);
     string GenerateRefreshToken();
     ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}