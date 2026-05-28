using Kongroo.Identity.Domain;
using Shouldly;

namespace Kongroo.CloudGames.UnitTests.Identity.Domain;

public sealed class UserTests
{
    [Fact]
    public void Create_WithValidValues_ShouldInitializeUserWithDefaultRole()
    {
        // Arrange
        var username = Username.From("kongroo");
        var email = Email.From("kongroo@example.com");
        var passwordHash = PasswordHash.From("password-hash");
        var name = PersonName.From("Kongroo");

        // Act
        var user = User.Create(username, email, passwordHash, name);

        // Assert
        user.Role.ShouldBe(UserRole.User);
    }

    [Fact]
    public void Create_WithValidValues_ShouldRaiseCreatedEvent()
    {
        // Arrange
        var username = Username.From("kongroo");
        var email = Email.From("kongroo@example.com");
        var passwordHash = PasswordHash.From("password-hash");
        var name = PersonName.From("Kongroo");

        // Act
        var user = User.Create(username, email, passwordHash, name);

        // Assert
        var domainEvent = user.DomainEvents.Single().ShouldBeOfType<UserCreatedDomainEvent>();
        domainEvent.UserId.ShouldBe(user.Id);
    }

    [Fact]
    public void Create_WithMixedCaseUsernameAndEmail_ShouldStoreCanonicalLowercaseValues()
    {
        // Arrange
        var username = Username.From("KongRoo");
        var email = Email.From("KongRoo@Example.COM");

        // Act
        var user = User.Create(username, email, PasswordHash.From("password-hash"), PersonName.From("Kongroo"));

        // Assert
        user.Username.Value.ShouldBe("kongroo");
        user.Email.Value.ShouldBe("kongroo@example.com");
    }

    [Fact]
    public void Create_WithWhitespaceAroundInputs_ShouldTrimSupportedFields()
    {
        // Arrange
        var username = Username.From("  kongroo  ");
        var email = Email.From("  kongroo@example.com  ");
        var name = PersonName.From("  Kongroo Cloud Games  ");

        // Act
        var user = User.Create(username, email, PasswordHash.From("password-hash"), name);

        // Assert
        user.Username.Value.ShouldBe("kongroo");
        user.Email.Value.ShouldBe("kongroo@example.com");
        user.Name.Value.ShouldBe("Kongroo Cloud Games");
    }

    [Fact]
    public void GrantAdmin_WhenUserRoleIsUser_ShouldPromoteUser()
    {
        // Arrange
        var user = CreateUser();
        user.ClearDomainEvents();

        // Act
        user.GrantAdmin();

        // Assert
        user.Role.ShouldBe(UserRole.Admin);
    }

    [Fact]
    public void GrantAdmin_WhenUserRoleIsUser_ShouldRaiseRoleChangedEvent()
    {
        // Arrange
        var user = CreateUser();
        user.ClearDomainEvents();

        // Act
        user.GrantAdmin();

        // Assert
        var domainEvent = user.DomainEvents.Single().ShouldBeOfType<UserRoleChangedDomainEvent>();
        domainEvent.UserId.ShouldBe(user.Id);
        domainEvent.PreviousRole.ShouldBe(UserRole.User);
        domainEvent.CurrentRole.ShouldBe(UserRole.Admin);
    }

    [Fact]
    public void GrantAdmin_WhenUserIsAlreadyAdmin_ShouldKeepUserAsAdmin()
    {
        // Arrange
        var user = CreateUser();
        user.GrantAdmin();
        user.ClearDomainEvents();

        // Act
        user.GrantAdmin();

        // Assert
        user.Role.ShouldBe(UserRole.Admin);
    }

    [Fact]
    public void GrantAdmin_WhenUserIsAlreadyAdmin_ShouldNotRaiseAnotherRoleChangedEvent()
    {
        // Arrange
        var user = CreateUser();
        user.GrantAdmin();
        user.ClearDomainEvents();

        // Act
        user.GrantAdmin();

        // Assert
        user.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void RevokeAdmin_WhenUserRoleIsAdmin_ShouldDemoteUser()
    {
        // Arrange
        var user = CreateUser();
        user.GrantAdmin();
        user.ClearDomainEvents();

        // Act
        user.RevokeAdmin();

        // Assert
        user.Role.ShouldBe(UserRole.User);
    }

    [Fact]
    public void RevokeAdmin_WhenUserRoleIsAdmin_ShouldRaiseRoleChangedEvent()
    {
        // Arrange
        var user = CreateUser();
        user.GrantAdmin();
        user.ClearDomainEvents();

        // Act
        user.RevokeAdmin();

        // Assert
        var domainEvent = user.DomainEvents.Single().ShouldBeOfType<UserRoleChangedDomainEvent>();
        domainEvent.UserId.ShouldBe(user.Id);
        domainEvent.PreviousRole.ShouldBe(UserRole.Admin);
        domainEvent.CurrentRole.ShouldBe(UserRole.User);
    }

    [Fact]
    public void RevokeAdmin_WhenUserIsAlreadyUser_ShouldKeepUserAsUser()
    {
        // Arrange
        var user = CreateUser();
        user.ClearDomainEvents();

        // Act
        user.RevokeAdmin();

        // Assert
        user.Role.ShouldBe(UserRole.User);
    }

    [Fact]
    public void RevokeAdmin_WhenUserIsAlreadyUser_ShouldNotRaiseRoleChangedEvent()
    {
        // Arrange
        var user = CreateUser();
        user.ClearDomainEvents();

        // Act
        user.RevokeAdmin();

        // Assert
        user.DomainEvents.ShouldBeEmpty();
    }

    private static User CreateUser() =>
        User.Create(
            Username.From("kongroo"),
            Email.From("kongroo@example.com"),
            PasswordHash.From("password-hash"),
            PersonName.From("Kongroo")
        );
}

