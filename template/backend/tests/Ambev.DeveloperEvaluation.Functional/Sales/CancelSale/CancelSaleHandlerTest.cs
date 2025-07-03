using System;
using System.Threading;
using System.Threading.Tasks;

using Ambev.DeveloperEvaluation.Application.Interfaces;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Domain.Entities;

using Bogus;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Sales.CancelSale
{
    public class CancelSaleHandlerTests
    {
        private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly ILogger<CancelSaleHandler> _logger = Substitute.For<ILogger<CancelSaleHandler>>();

        private CancelSaleHandler CreateHandler() =>
            new CancelSaleHandler(_saleRepository, _unitOfWork, _logger);

        [Fact]
        public async Task Handle_ShouldCancelSale_WhenSaleExists()
        {
            // Arrange
            var saleId = Guid.NewGuid();
            var sale = Substitute.For<Sale>();

            _saleRepository.GetByIdAsync(saleId).Returns(sale);

            var command = new CancelSaleCommand(saleId);
            var handler = CreateHandler();

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            sale.Received(1).Cancel();
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            _logger.Received().LogInformation("Sale with Id: {SaleId} cancelled successfully.", saleId);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenSaleNotFound()
        {
            // Arrange
            var saleId = Guid.NewGuid();
            _saleRepository.GetByIdAsync(saleId).Returns((Sale?)null);

            var command = new CancelSaleCommand(saleId);
            var handler = CreateHandler();

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Sale with Id {saleId} not found.");
        }
    }
}
