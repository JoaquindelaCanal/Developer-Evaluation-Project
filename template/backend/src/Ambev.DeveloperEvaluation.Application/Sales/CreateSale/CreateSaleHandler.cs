using Ambev.DeveloperEvaluation.Application.Common.Contracts.Sales;
using Ambev.DeveloperEvaluation.Application.Common.Exceptions;
using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.Application.Interfaces;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events.Sales;

using AutoMapper;

using MediatR;

using Microsoft.Extensions.Logging;

using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale
{
    public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, SaleDto>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBus _rebusBus;
        private readonly ILogger<CreateSaleHandler> _logger;

        public CreateSaleHandler(
            ISaleRepository saleRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IBus rebusBus,
            ILogger<CreateSaleHandler> logger)
        {
            _saleRepository = saleRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _rebusBus = rebusBus;
            _logger = logger;
        }

        public async Task<SaleDto> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new sale for CustomerId: {CustomerId}", request.CustomerId);

            // bsic validation
            if (string.IsNullOrWhiteSpace(request.CustomerName))
            {
                throw new BadRequestException("Customer name is required.");
            }
            if (request.Items == null || !request.Items.Any())
            {
                throw new BadRequestException("Sale must have at least one item.");
            }

            var sale = new Sale(
                request.CustomerId,
                request.CustomerName,
                request.BranchId,
                request.BranchName,
                request.SaleNumber
            );

            foreach (var itemCommand in request.Items)
            {
                var saleItem = new SaleItem(
                    itemCommand.ProductId,
                    itemCommand.ProductName,
                    itemCommand.Quantity,
                    itemCommand.UnitPrice
                );

                if (saleItem.Quantity <= 0)
                {
                    throw new BadRequestException($"Quantity for product {saleItem.ProductId} must be greater than zero.");
                }
                if (saleItem.UnitPrice <= 0)
                {
                    throw new BadRequestException($"Unit price for product {saleItem.ProductId} must be greater than zero.");
                }

                sale.AddItem(saleItem);
            }

            sale.RecalculateTotal();

            await _saleRepository.AddAsync(sale);
            await _unitOfWork.SaveChangesAsync(cancellationToken); 

            _logger.LogInformation("Sale created successfully with Id: {SaleId}", sale.Id);

            //Publish Domain events: await _mediator.Publish(new SaleCreatedEvent(sale), cancellationToken);

            var integrationEvent = new SaleCreatedIntegrationEvent(sale);
            
            //Publish the integration event using Rebus
            await _rebusBus.Publish(integrationEvent);

            return _mapper.Map<SaleDto>(sale);
        }
    }
}
