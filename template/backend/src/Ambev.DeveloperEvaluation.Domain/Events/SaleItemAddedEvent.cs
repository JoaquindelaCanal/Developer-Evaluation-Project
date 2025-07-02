using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events.Sales
{
    public record SaleItemAddedEvent(
        Guid SaleId,
        Guid SaleItemId,
        Guid ProductId,
        int Quantity,
        decimal UnitPrice,
        decimal DiscountPercentage
    ) : INotification;
}
