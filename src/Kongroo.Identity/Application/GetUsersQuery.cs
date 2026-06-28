using Kongroo.BuildingBlocks.Application;

namespace Kongroo.Identity.Application;

public sealed record GetUsersQuery() : IQuery<IReadOnlyList<GetUserResponse>>;
