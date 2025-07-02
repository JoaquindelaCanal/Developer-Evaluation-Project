using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.Application.Interfaces;
using Ambev.DeveloperEvaluation.Domain.Entities;

using AutoMapper;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale
{
    public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, SaleDto>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateSaleCommandHandler> _logger;

        public CreateSaleCommandHandler(
            ISaleRepository saleRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateSaleCommandHandler> logger)
        {
            _saleRepository = saleRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SaleDto> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new sale for CustomerId: {CustomerId}", request.CustomerId);

            // Create the Sale entity
            var sale = new Sale(
                request.CustomerId,
                request.CustomerName,
                request.BranchId,
                request.BranchName,
                request.SaleNumber
            );

            // Add items to the sale
            foreach (var itemCommand in request.Items)
            {
                var saleItem = new SaleItem(
                    itemCommand.ProductId,
                    itemCommand.ProductName,
                    itemCommand.Quantity,
                    itemCommand.UnitPrice
                );
                sale.AddItem(saleItem);
            }

            await _saleRepository.AddAsync(sale);
            await _unitOfWork.SaveChangesAsync(cancellationToken); 

            _logger.LogInformation("Sale created successfully with Id: {SaleId}", sale.Id);

            return _mapper.Map<SaleDto>(sale);
        }
    }
}
