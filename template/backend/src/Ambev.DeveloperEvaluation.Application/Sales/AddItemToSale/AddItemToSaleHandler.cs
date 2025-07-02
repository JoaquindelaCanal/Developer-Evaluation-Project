using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.Application.Interfaces;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.AddItemToSale
{
    public class AddItemToSaleHandler : IRequestHandler<AddItemToSaleCommand, SaleDto>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AddItemToSaleHandler> _logger;

        public AddItemToSaleHandler(
            ISaleRepository saleRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<AddItemToSaleHandler> logger)
        {
            _saleRepository = saleRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SaleDto> Handle(AddItemToSaleCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Adding item to sale {SaleId} for Product: {ProductId}", request.SaleId, request.ProductId);

            var sale = await _saleRepository.GetByIdAsync(request.SaleId);
            if (sale == null)
            {
                _logger.LogWarning("Sale with Id: {SaleId} not found for adding item.", request.SaleId);
                throw new InvalidOperationException($"Sale with Id {request.SaleId} not found.");
            }

            var newItem = new SaleItem(
                request.ProductId,
                request.ProductName,
                request.Quantity,
                request.UnitPrice
            );

            sale.AddItem(newItem); //raises SaleItemAddedEvent

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Item {ProductId} added to sale {SaleId} successfully.", request.ProductId, request.SaleId);

            return _mapper.Map<SaleDto>(sale);
        }
    }
}
