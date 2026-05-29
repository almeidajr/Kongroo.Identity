using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Kongroo.Identity.Application;
using Kongroo.Identity.Application.Abstractions;
using Kongroo.Identity.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Kongroo.Identity.Infrastructure;

public sealed class JwtAccessTokenIssuer(IOptions<JwtOptions> options, TimeProvider timeProvider) : IAccessTokenIssuer
{
    private readonly JwtOptions _jwtOptions = options.Value;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public AuthenticateUserResponse IssueToken(User user)
    {
        var issuedAt = timeProvider.GetUtcNow();
        var expiresAt = issuedAt.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = CreateClaimsIdentity(user),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            IssuedAt = issuedAt.UtcDateTime,
            NotBefore = issuedAt.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(_jwtOptions.CreateSigningKey(), SecurityAlgorithms.HmacSha256),
        };

        var securityToken = _tokenHandler.CreateToken(tokenDescriptor);

        return new AuthenticateUserResponse(
            _tokenHandler.WriteToken(securityToken),
            JwtBearerDefaults.AuthenticationScheme,
            Convert.ToInt32((expiresAt - issuedAt).TotalSeconds)
        );
    }

    private static ClaimsIdentity CreateClaimsIdentity(User user) =>
        new([
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username.Value),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim(JwtRegisteredClaimNames.Name, user.Name.Value),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        ]);
}
