using Common.SharedKernel.Application.Messaging;
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
    : IQueryHandler<RefreshTokenQuery, string>
{
    public async Task<Result<string>> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
    {
        var httpContext = accessor.HttpContext;
        var tokenFromCookie = httpContext?.Request.Cookies[".authToken"];

        if (string.IsNullOrWhiteSpace(tokenFromCookie))
        {
            return Result.Failure<string>(Error.Problem("Auth.Unauthorized", "The user is not authorized."));
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(configuration["CustomSettings:Secret"]!);
        var ExpireTimeToken = Encoding.ASCII.GetBytes(configuration["CustomSettings:ExpireTimeToken"]!);

        try
        {
            var principal = tokenHandler.ValidateToken(tokenFromCookie, new TokenValidationParameters
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
                return Result.Failure<string>(Error.Problem("Auth.Unauthorized", "The user is not authorized.")
);
            }

            var user = await userRepository.GetByUserNameAsync(userName);
            if (user is null)
            {
                return Result.Failure<string>(Error.NotFound(
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

            return Result.Success<string>(tokenHandler.WriteToken(newToken));

        }
        catch (SecurityTokenException)
        {
            return Result.Failure<string>(Error.Problem("Auth.Unauthorized", "The user is not authorized.")
);
        }
    }
}