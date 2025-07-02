using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Application.Interfaces;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale
{
    public class CancelSaleHandler : IRequestHandler<CancelSaleCommand>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CancelSaleHandler> _logger;

        public CancelSaleHandler(
            ISaleRepository saleRepository,
            IUnitOfWork unitOfWork,
            ILogger<CancelSaleHandler> logger)
        {
            _saleRepository = saleRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handle(CancelSaleCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to cancel sale with Id: {SaleId}", request.SaleId);

            var sale = await _saleRepository.GetByIdAsync(request.SaleId);
            if (sale == null)
            {
                _logger.LogWarning("Sale with Id: {SaleId} not found for cancellation.", request.SaleId);
                // Consider throwing a specific NotFoundException or returning an error result
                throw new InvalidOperationException($"Sale with Id {request.SaleId} not found.");
            }

            sale.Cancel(); //change status and raise SaleCancelledEvent and SaleItemCancelledEvents
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Sale with Id: {SaleId} cancelled successfully.", request.SaleId);
        }
    }
}
