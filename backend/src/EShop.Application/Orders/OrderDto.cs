using EShop.Domain.Orders;

namespace EShop.Application.Orders;

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    string TrackingNumber,
    DateTime PurchaseDate,
    DateTime? EstimatedDelivery,
    decimal Total,
    string? PaymentCard,
    List<OrderItemDetailDto> Items
)
{
    public static OrderDto FromOrder(Order o, Dictionary<Guid, string> productNames) => new(
        o.Id.Value,
        o.CustomerId.Value,
        o.Status.ToString(),
        o.TrackingNumber.Value,
        o.PurchaseDate,
        o.EstimatedDelivery,
        o.Total.Amount,
        o.PaymentCard?.ToString(),
        o.Items.Select(i => new OrderItemDetailDto(
            i.ProductId.Value,
            productNames.GetValueOrDefault(i.ProductId.Value, "Unknown Product"),
            i.Quantity,
            i.UnitPrice.Amount,
            i.TotalPrice.Amount
        )).ToList()
    );
}
