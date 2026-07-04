using Kongroo.BuildingBlocks.Application;

namespace Kongroo.Identity.Contracts;

public sealed record UserRoleChangedIntegrationEvent(Guid UserId, string PreviousRole, string CurrentRole)
    : IntegrationEvent;
