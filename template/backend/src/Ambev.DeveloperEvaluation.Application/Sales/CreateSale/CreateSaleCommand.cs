using Ambev.DeveloperEvaluation.Application.DTOs;

using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale
{
    public record CreateSaleCommand(
        Guid CustomerId,
        string CustomerName,
        Guid BranchId,
        string BranchName,
        string SaleNumber,
        List<CreateSaleItemCommand> Items
    ) : IRequest<SaleDto>;
}
