using Kongroo.BuildingBlocks.Application;

namespace Kongroo.BuildingBlocks.Contracts;

public sealed record UserRoleChangedIntegrationEvent(Guid UserId, string PreviousRole, string CurrentRole)
    : IntegrationEvent;
