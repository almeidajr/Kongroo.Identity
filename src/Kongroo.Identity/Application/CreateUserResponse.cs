using System.ComponentModel;

namespace Kongroo.Identity.Application;

public sealed record CreateUserResponse(
    [property: Description("Unique identifier assigned to the created user.")] Guid Id,
    [property: Description("Unique sign-in name chosen by the user.")] string Username,
    [property: Description("Email address associated with the user account.")] string Email,
    [property: Description("Display name shown for the user profile.")] string Name
);
