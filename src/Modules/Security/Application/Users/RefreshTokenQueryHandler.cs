using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Security.Application.Contracts.Users;
using Security.Domain.Users;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Security.Application.Users;

public sealed class RefreshTokenQueryHandler(
    IHttpContextAccessor accessor,
    IUserRepository userRepository,
    IConfiguration configuration)
    : IQueryHandler<RefreshTokenQuery, UserForAuthenticationDto>
{
    public async Task<Result<UserForAuthenticationDto>> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
    {
        var httpContext = accessor.HttpContext;
        var token = httpContext?.Request.Cookies[".authToken"];

        if (string.IsNullOrWhiteSpace(token))
        {
            var authHeader = httpContext?.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader["Bearer ".Length..].Trim();
            }
            else
            {
                return Result.Failure<UserForAuthenticationDto>(Error.Problem("Auth.Unauthorized", "The user is not authorized."));
            }
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(configuration["CustomSettings:Secret"]!);
        var ExpireTimeToken = Encoding.ASCII.GetBytes(configuration["CustomSettings:ExpireTimeToken"]!);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                RequireExpirationTime = true
            }, out _);

            var userName = principal.Identity?.Name;

            if (string.IsNullOrWhiteSpace(userName))
            {
                return Result.Failure<UserForAuthenticationDto>(Error.Problem("Auth.Unauthorized", "The user is not authorized.")
);
            }

            var user = await userRepository.GetByUserNameAsync(userName);
            if (user is null)
            {
                return Result.Failure<UserForAuthenticationDto>(Error.NotFound(
                    "Auth.User.NotFound",
                    "The user does not exist."));
            }

            var newToken = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            var userForAuthenticationDto = new UserForAuthenticationDto()
            {
                Id = user.Id,
                UserName = user.UserName,
                Name = user.Name,
                MiddleName = user.MiddleName,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Token = tokenHandler.WriteToken(newToken)
            };

            return Result.Success<UserForAuthenticationDto>(userForAuthenticationDto);

        }
        catch (SecurityTokenException)
        {
            return Result.Failure<UserForAuthenticationDto>(Error.Problem("Auth.Unauthorized", "The user is not authorized.")
);
        }
    }
}