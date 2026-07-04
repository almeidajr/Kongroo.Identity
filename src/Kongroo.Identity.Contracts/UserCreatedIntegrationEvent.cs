using Kongroo.BuildingBlocks.Application;

namespace Kongroo.Identity.Contracts;

public sealed record UserCreatedIntegrationEvent(Guid UserId, string Email, string Name) : IntegrationEvent;
