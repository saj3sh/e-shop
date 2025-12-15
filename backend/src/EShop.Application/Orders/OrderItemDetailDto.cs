namespace EShop.Application.Orders;

public record OrderItemDetailDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal TotalPrice);
