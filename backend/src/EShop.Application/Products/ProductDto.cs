using EShop.Domain.Products;

namespace EShop.Application.Products;

public record ProductDto(Guid Id, string Name, decimal Price, string Sku, string ManufacturedFrom, string ShippedFrom)
{
    public static explicit operator ProductDto(Product p) => new(
        p.Id.Value,
        p.Name,
        p.Price.Amount,
        p.Sku.Value,
        p.ManufacturedFrom,
        p.ShippedFrom
    );
}
