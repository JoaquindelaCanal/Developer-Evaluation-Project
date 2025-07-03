using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events.Sales
{
    public record SaleItemCancelledEvent(
        Guid SaleId,
        Guid SaleItemId
    ) : INotification;
}