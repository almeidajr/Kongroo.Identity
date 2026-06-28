using Kongroo.BuildingBlocks.Application;

namespace Kongroo.Identity.Application;

public sealed record GetUserQuery(Guid UserId) : IQuery<GetUserResponse>;
