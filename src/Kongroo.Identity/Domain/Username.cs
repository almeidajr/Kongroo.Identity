namespace Kongroo.Identity.Domain;

public sealed record Username(string Value)
{
    public const int MinLength = 4;
    public const int MaxLength = 32;

    public static Username From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalizedValue = value.Trim().ToLowerInvariant();

        return normalizedValue.Length switch
        {
            < MinLength => throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Username must be at least {MinLength} characters long."
            ),
            > MaxLength => throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Username must be at most {MaxLength} characters long."
            ),
            _ => new Username(normalizedValue),
        };
    }
}
