namespace EShop.Domain.Orders;

/// <summary>
/// Represents a masked payment card for secure storage and display.
/// Full card numbers are never stored - only the last 4 digits are retained.
/// </summary>
public sealed record PaymentCard
{
    private const string MaskPrefix = "****";
    private const int MinimumDigitsForMasking = 4;

    public string MaskedValue { get; }
    public string? CardType { get; }

    private PaymentCard(string maskedValue, string? cardType)
    {
        MaskedValue = maskedValue;
        CardType = cardType;
    }

    /// <summary>
    /// Creates a masked payment card from raw input, preserving only the last 4 digits.
    /// </summary>
    /// <param name="cardNumber">Raw card number string (may contain spaces, dashes)</param>
    /// <param name="cardType">Type of card (Visa, Mastercard, etc.)</param>
    /// <returns>PaymentCard with masked value, or null if input is null/empty</returns>
    public static PaymentCard? CreateMasked(string? cardNumber, string? cardType = null)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            return null;

        var cleaned = cardNumber.Replace(" ", "")
                                .Replace("-", "")
                                .Replace("_", "");
        var masked = cleaned.Length >= MinimumDigitsForMasking
            ? $"{MaskPrefix}{cleaned[^MinimumDigitsForMasking..]}"
            : MaskPrefix;

        return new PaymentCard(masked, cardType);
    }

    public override string ToString() => CardType != null
        ? $"{CardType} {MaskedValue}"
        : MaskedValue;
}
