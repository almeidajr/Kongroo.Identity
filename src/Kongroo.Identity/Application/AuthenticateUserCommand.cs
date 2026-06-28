using Kongroo.BuildingBlocks.Application;

namespace Kongroo.Identity.Application;

public sealed record AuthenticateUserCommand(string Username, string Password) : ICommand<AuthenticateUserResponse?>;
