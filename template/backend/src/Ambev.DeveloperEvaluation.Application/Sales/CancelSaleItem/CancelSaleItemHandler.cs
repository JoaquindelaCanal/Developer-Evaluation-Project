﻿using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.Application.Interfaces;

using AutoMapper;

using MediatR;

using Microsoft.Extensions.Logging;

using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem
{
    public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, SaleDto>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBus _rebusBus;
        private readonly ILogger<CancelSaleItemHandler> _logger;

        public CancelSaleItemHandler(
            ISaleRepository saleRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IBus rebusBus,
            ILogger<CancelSaleItemHandler> logger)
        {
            _saleRepository = saleRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _rebusBus = rebusBus;
            _logger = logger;
        }

        public async Task<SaleDto> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to cancel sale item {SaleItemId} in sale {SaleId}", request.SaleItemId, request.SaleId);

            var sale = await _saleRepository.GetByIdAsync(request.SaleId);
            if (sale == null)
            {
                _logger.LogWarning("Sale with Id: {SaleId} not found for item cancellation.", request.SaleId);
                throw new InvalidOperationException($"Sale with Id {request.SaleId} not found.");
            }

            sale.CancelItem(request.SaleItemId); //raises SaleItemCancelledEvent.

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Sale item {SaleItemId} in sale {SaleId} cancelled successfully.", request.SaleItemId, request.SaleId);

            //var integrationEvent = new SaleItemCancelledIntegrationEventDto(sale);

            //Publish the integration event using Rebus
            //await _rebusBus.Publish(integrationEvent);

            return _mapper.Map<SaleDto>(sale);
        }
    }
}
