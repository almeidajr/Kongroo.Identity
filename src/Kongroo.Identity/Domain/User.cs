using Kongroo.BuildingBlocks.Domain;

namespace Kongroo.Identity.Domain;

public sealed class User : Entity<UserId>
{
    private User() { }

    public required Username Username { get; init; }

    public required Email Email { get; init; }

    public required PasswordHash PasswordHash { get; init; }

    public required SecurityStamp SecurityStamp { get; init; }

    public required PersonName Name { get; init; }

    public required UserRole Role { get; set; }

    public static User Create(Username username, Email email, PasswordHash passwordHash, PersonName name)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(passwordHash);
        ArgumentNullException.ThrowIfNull(name);

        var user = new User
        {
            Id = UserId.Create(),
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            SecurityStamp = SecurityStamp.Create(),
            Name = name,
            Role = UserRole.User,
        };
        user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id));
        return user;
    }

    public void GrantAdmin() => ChangeRole(UserRole.Admin);

    public void RevokeAdmin() => ChangeRole(UserRole.User);

    private void ChangeRole(UserRole role)
    {
        if (Role == role)
        {
            return;
        }

        var previousRole = Role;
        Role = role;
        RaiseDomainEvent(new UserRoleChangedDomainEvent(Id, previousRole, Role));
    }
}
