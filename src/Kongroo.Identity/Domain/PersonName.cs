namespace Kongroo.Identity.Domain;

public sealed record PersonName(string Value)
{
    public const int MinLength = 2;
    public const int MaxLength = 256;

    public static PersonName From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalizedValue = value.Trim();

        return normalizedValue.Length switch
        {
            < MinLength => throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Name must be at least {MinLength} characters long."
            ),
            > MaxLength => throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Name must be at most {MaxLength} characters long."
            ),
            _ => new PersonName(normalizedValue),
        };
    }
}

