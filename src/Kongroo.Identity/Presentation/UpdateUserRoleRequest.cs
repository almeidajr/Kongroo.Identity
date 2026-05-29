using System.ComponentModel;
using Kongroo.Identity.Domain;

namespace Kongroo.Identity.Presentation;

public sealed record UpdateUserRoleRequest(
    [property: Description("Role to assign to the user account.")] UserRole Role
);
