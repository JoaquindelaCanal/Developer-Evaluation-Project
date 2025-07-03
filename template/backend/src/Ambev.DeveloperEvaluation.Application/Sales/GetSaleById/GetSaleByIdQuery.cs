using Ambev.DeveloperEvaluation.Application.DTOs;

using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSaleById
{
    public record GetSaleByIdQuery(Guid SaleId) : IRequest<SaleDto>;
}
