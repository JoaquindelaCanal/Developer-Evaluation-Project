using Ambev.DeveloperEvaluation.Application.DTOs;

using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales
{
    public record ListSalesQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string SortBy = "SaleDate", // default sort
        string Search = null
    ) : IRequest<IEnumerable<SaleDto>>;
}
