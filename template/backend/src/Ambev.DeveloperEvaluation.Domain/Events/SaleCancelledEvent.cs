using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events.Sales
{
    public record SaleCancelledEvent(
        Guid SaleId,
        string SaleNumber,
        DateTime CancellationDate
    ) : INotification;
}