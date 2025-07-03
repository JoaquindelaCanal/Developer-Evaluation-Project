using MediatR;
using Ambev.DeveloperEvaluation.Application.Common.Models.QueryParameters;
using Ambev.DeveloperEvaluation.Application.DTOs;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales
{
    public class ListSalesQuery : IRequest<ApplicationPaginatedList<SaleDto>>
    {
        public int Page { get; set; }
        public int Size { get; set; }
        public List<SortOption> SortOptions { get; set; } = new List<SortOption>();      
        public Dictionary<string, List<FilterOption>> Filters { get; set; } = new Dictionary<string, List<FilterOption>>();
    }
}
