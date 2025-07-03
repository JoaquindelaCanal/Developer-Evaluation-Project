using Ambev.DeveloperEvaluation.Application.DTOs;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.AddItemToSale
{
    public record AddItemToSaleCommand(
        Guid SaleId,
        Guid ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice
    ) : IRequest<SaleDto>;
}
