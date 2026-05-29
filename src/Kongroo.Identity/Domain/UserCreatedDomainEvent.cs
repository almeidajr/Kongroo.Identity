using Kongroo.BuildingBlocks.Domain;

namespace Kongroo.Identity.Domain;

public record UserCreatedDomainEvent(UserId UserId) : DomainEvent;
