using Kongroo.Identity.Domain;

namespace Kongroo.Identity.Application.Abstractions;

public interface IAccessTokenIssuer
{
    AuthenticateUserResponse IssueToken(User user);
}
