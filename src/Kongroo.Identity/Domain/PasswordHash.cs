namespace Kongroo.Identity.Domain;

public sealed record PasswordHash(string Value)
{
    public const int MaxLength = 256;

    public static PasswordHash From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Password hash must be at most {MaxLength} characters long."
            );
        }

        return new PasswordHash(value);
    }
}
