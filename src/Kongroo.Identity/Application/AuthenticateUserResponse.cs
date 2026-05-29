using System.ComponentModel;

namespace Kongroo.Identity.Application;

public sealed record AuthenticateUserResponse(
    [property: Description("Bearer access token that authenticates the caller.")] string AccessToken,
    [property: Description("Token type returned by the identity service.")] string TokenType,
    [property: Description("Number of seconds before the access token expires.")] int ExpiresIn
);
