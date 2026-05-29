using System.Net.Mail;

namespace Kongroo.Identity.Domain;

public sealed record Email(string Value)
{
    public const int MaxLength = 256;

    public static Email From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalizedValue = value.Trim().ToLowerInvariant();

        if (normalizedValue.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Email must be at most {MaxLength} characters long."
            );
        }

        try
        {
            var mailAddress = new MailAddress(normalizedValue);
            return new Email(mailAddress.Address);
        }
        catch (FormatException exception)
        {
            throw new ArgumentException("Email format is invalid.", nameof(value), exception);
        }
    }
}
