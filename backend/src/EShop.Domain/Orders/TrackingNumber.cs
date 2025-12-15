using System.Text.RegularExpressions;

namespace EShop.Domain.Orders;

/// <summary>
/// tracking number value object: "Unq" + 9 digits + 2-letter country code
/// </summary>
public partial record TrackingNumber
{
    public string Value { get; }

    private TrackingNumber(string value)
    {
        Value = value;
    }

    public static TrackingNumber Generate(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
            throw new ArgumentException("country code must be 2 letters");

        var random = new Random();
        var digits = random.Next(100000000, 999999999);
        var tracking = $"Unq{digits}{countryCode.ToUpperInvariant()}";

        return new TrackingNumber(tracking);
    }

    public static TrackingNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("tracking number cannot be empty");

        if (!TrackingRegex().IsMatch(value))
            throw new ArgumentException($"invalid tracking number format: {value}");

        return new TrackingNumber(value);
    }

    [GeneratedRegex(@"^Unq\d{9}[A-Z]{2}$")]
    private static partial Regex TrackingRegex();

    public override string ToString() => Value;
}
