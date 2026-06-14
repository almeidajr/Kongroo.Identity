using Kongroo.BuildingBlocks.Application;

namespace Kongroo.BuildingBlocks.Contracts;

public sealed record UserCreatedIntegrationEvent(Guid UserId, string Email, string Name) : IntegrationEvent;
