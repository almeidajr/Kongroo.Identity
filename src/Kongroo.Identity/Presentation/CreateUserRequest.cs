using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Kongroo.Identity.Domain;

namespace Kongroo.Identity.Presentation;

public sealed record CreateUserRequest(
    [property: Required]
    [property: MinLength(Username.MinLength)]
    [property: MaxLength(Username.MaxLength)]
    [property: Description("Unique sign-in name for the new user.")]
        string Username,
    [property: Required]
    [property: EmailAddress]
    [property: MaxLength(Email.MaxLength)]
    [property: Description("Email address used for account communication and sign-in.")]
        string Email,
    [property: Required]
    [property: MinLength(8)]
    [property: MaxLength(128)]
    [property: RegularExpression(
        @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^\w\s]).+$",
        ErrorMessage = "Password must include letters, numbers, and special characters."
    )]
    [property: DataType(DataType.Password)]
    [property: Description("Plain-text password supplied during account registration.")]
        string Password,
    [property: Required]
    [property: MinLength(PersonName.MinLength)]
    [property: MaxLength(PersonName.MaxLength)]
    [property: Description("Display name shown for the user profile.")]
        string Name
);

