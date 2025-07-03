using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events.Sales
{
    public record SaleCompletedEvent(
        Guid SaleId,
        string SaleNumber,
        DateTime CompletionDate
    ) : INotification;
}