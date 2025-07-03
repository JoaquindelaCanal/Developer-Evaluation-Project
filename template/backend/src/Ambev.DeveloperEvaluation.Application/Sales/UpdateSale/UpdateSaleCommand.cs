using Ambev.DeveloperEvaluation.Application.DTOs;

using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale
{
    public record UpdateSaleCommand(
            Guid SaleId,
            string CustomerName,
            string BranchName
        ) : IRequest<SaleDto>;
}
