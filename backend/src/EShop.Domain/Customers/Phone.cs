using System.Text.RegularExpressions;

namespace EShop.Domain.Customers;

/// <summary>
/// phone value object, basic validation
/// </summary>
public partial record Phone
{
    public string Value { get; }

    private Phone(string value)
    {
        Value = value;
    }

    public static Phone Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("phone cannot be empty");

        phone = PhoneCleanRegex().Replace(phone, "");

        if (phone.Length < 10)
            throw new ArgumentException("phone too short");

        return new Phone(phone);
    }

    [GeneratedRegex(@"[^\d+]")]
    private static partial Regex PhoneCleanRegex();

    public override string ToString() => Value;
}
