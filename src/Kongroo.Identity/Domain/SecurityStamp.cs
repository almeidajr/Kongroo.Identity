using System.Security.Cryptography;

namespace Kongroo.Identity.Domain;

public sealed record SecurityStamp(string Value)
{
    public const int Length = 32;

    public static SecurityStamp Create() => new(RandomNumberGenerator.GetHexString(Length));

    public static SecurityStamp From(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length != Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Security stamp must be exactly {Length} characters long."
            );
        }

        return new SecurityStamp(value);
    }
}

