using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Kongroo.Identity.Domain;

namespace Kongroo.Identity.Presentation;

public sealed record CreateAccessTokenRequest(
    [property: Required]
    [property: MinLength(Username.MinLength)]
    [property: MaxLength(Username.MaxLength)]
    [property: Description("Unique sign-in name used to authenticate the user.")]
        string Username,
    [property: Required]
    [property: MaxLength(128)]
    [property: DataType(DataType.Password)]
    [property: Description("Plain-text password supplied during sign-in.")]
        string Password
);
