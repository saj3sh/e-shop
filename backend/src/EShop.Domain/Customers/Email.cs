using System.Text.RegularExpressions;

namespace EShop.Domain.Customers;

/// <summary>
/// email value object with validation
/// </summary>
public partial record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("email cannot be empty");

        email = email.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(email))
            throw new ArgumentException($"invalid email format: {email}");

        return new Email(email);
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    public override string ToString() => Value;
}
