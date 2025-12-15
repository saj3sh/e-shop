namespace EShop.Domain.Products;

/// <summary>
/// money value object with currency support
/// </summary>
public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("amount cannot be negative");

        return new Money(amount, currency);
    }

    public static Money ParseFromString(string? priceStr, string currency = "USD")
    {
        if (string.IsNullOrWhiteSpace(priceStr))
            return new Money(0, currency);

        var cleaned = priceStr.Trim().Replace("$", "").Replace("€", "").Replace("£", "").Trim();

        if (decimal.TryParse(cleaned, System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture, out var amount))
        {
            return Create(amount, currency);
        }

        return new Money(0, currency);
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("cannot add different currencies");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(int quantity)
    {
        return new Money(Amount * quantity, Currency);
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
