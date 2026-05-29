using System.ComponentModel.DataAnnotations;
using Kongroo.Identity.Domain;

namespace Kongroo.Identity.Infrastructure;

public sealed class BootstrapAdminOptions
{
    public const string SectionName = "BootstrapAdmin";

    [Required]
    [MinLength(Domain.Username.MinLength)]
    [MaxLength(Domain.Username.MaxLength)]
    public string Username { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(Domain.Email.MaxLength)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    [RegularExpression(
        @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^\w\s]).+$",
        ErrorMessage = "Password must include letters, numbers, and special characters."
    )]
    public string Password { get; init; } = string.Empty;

    [Required]
    [MinLength(PersonName.MinLength)]
    [MaxLength(PersonName.MaxLength)]
    public string Name { get; init; } = string.Empty;
}
