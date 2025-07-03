using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Application.Interfaces;
using Ambev.DeveloperEvaluation.Application.DTOs;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales
{
    public class ListSalesQueryHandler : IRequestHandler<ListSalesQuery, IEnumerable<SaleDto>>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ListSalesQueryHandler> _logger;

        public ListSalesQueryHandler(
            ISaleRepository saleRepository,
            IMapper mapper,
            ILogger<ListSalesQueryHandler> logger)
        {
            _saleRepository = saleRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<SaleDto>> Handle(ListSalesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Listing sales with PageNumber: {PageNumber}, PageSize: {PageSize}, SortBy: {SortBy}, Search: {Search}",
                request.PageNumber, request.PageSize, request.SortBy, request.Search);

            var sales = await _saleRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.Search,
                cancellationToken
            );

            return _mapper.Map<IEnumerable<SaleDto>>(sales);
        }
    }
}
