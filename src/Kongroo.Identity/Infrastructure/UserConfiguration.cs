using Kongroo.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kongroo.Identity.Infrastructure;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).HasConversion(id => id.Value, value => UserId.From(value));

        builder.HasIndex(user => user.Username).IsUnique();
        builder.HasIndex(user => user.Email).IsUnique();

        builder
            .Property(user => user.Username)
            .HasConversion(username => username.Value, value => Username.From(value))
            .HasMaxLength(Username.MaxLength);
        builder
            .Property(user => user.Email)
            .HasConversion(email => email.Value, value => Email.From(value))
            .HasMaxLength(Email.MaxLength);
        builder
            .Property(user => user.PasswordHash)
            .HasConversion(passwordHash => passwordHash.Value, value => PasswordHash.From(value))
            .HasMaxLength(PasswordHash.MaxLength);
        builder
            .Property(user => user.SecurityStamp)
            .HasConversion(securityStamp => securityStamp.Value, value => SecurityStamp.From(value))
            .HasMaxLength(SecurityStamp.Length)
            .IsFixedLength();
        builder
            .Property(user => user.Name)
            .HasConversion(name => name.Value, value => PersonName.From(value))
            .HasMaxLength(PersonName.MaxLength);
        builder.Property(user => user.Role).HasConversion<string>().HasMaxLength(16);
    }
}

