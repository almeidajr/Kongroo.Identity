using Kongroo.Identity.Domain;

namespace Kongroo.Identity.Application;

public sealed record UpdateUserRoleCommand(Guid ActingUserId, Guid TargetUserId, UserRole Role);

