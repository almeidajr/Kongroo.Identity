using Kongroo.Identity.Domain;

namespace Kongroo.Identity.Application;

public sealed record CreateUserCommand(string Username, string Email, string Password, string Name, UserRole Role);
