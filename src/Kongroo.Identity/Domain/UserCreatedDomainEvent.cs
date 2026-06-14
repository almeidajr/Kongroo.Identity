using Kongroo.BuildingBlocks.Domain;

namespace Kongroo.Identity.Domain;

public sealed record UserCreatedDomainEvent(UserId UserId, Email Email, PersonName Name) : DomainEvent;
