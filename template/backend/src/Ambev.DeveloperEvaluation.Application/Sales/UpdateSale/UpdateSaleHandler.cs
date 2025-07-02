using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.Application.Interfaces;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale
{
    public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, SaleDto>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateSaleHandler> _logger;

        public UpdateSaleHandler(
            ISaleRepository saleRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpdateSaleHandler> logger)
        {
            _saleRepository = saleRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SaleDto> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating sale with Id: {SaleId}", request.SaleId);

            var sale = await _saleRepository.GetByIdAsync(request.SaleId);
            if (sale == null)
            {
                _logger.LogWarning("Sale with Id: {SaleId} not found for update.", request.SaleId);
                return null;
            }

            sale.UpdateBranchDetails(request.BranchName, null);
            sale.UpdateCustomerDetails(request.CustomerName, null);

            await _unitOfWork.SaveChangesAsync(cancellationToken); // dispatches domain events

            _logger.LogInformation("Sale with Id: {SaleId} updated successfully.", sale.Id);

            return _mapper.Map<SaleDto>(sale);
        }
    }
}
