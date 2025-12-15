using System.Security.Cryptography;
using System.Text;

namespace EShop.Domain.Products;

/// <summary>
/// sku value object, stable hash of product attributes
/// </summary>
public record Sku
{
    public string Value { get; }

    private Sku(string value)
    {
        Value = value;
    }

    public static Sku Generate(string name, string manufacturedFrom)
    {
        var input = $"{name.ToLowerInvariant()}:{manufacturedFrom.ToLowerInvariant()}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var sku = "SKU" + Convert.ToHexString(hash)[..12];

        return new Sku(sku);
    }

    public static Sku Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("sku cannot be empty");

        return new Sku(value);
    }

    public override string ToString() => Value;
}
