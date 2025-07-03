using MediatR;

using System;

namespace Ambev.DeveloperEvaluation.Domain.Events.Sales
{
    public record SaleCreatedEvent(
        Guid SaleId,
        string SaleNumber,
        Guid CustomerId,
        string CustomerName,
        Guid BranchId,
        string BranchName,
        DateTime SaleDate
    ) : INotification;
}
