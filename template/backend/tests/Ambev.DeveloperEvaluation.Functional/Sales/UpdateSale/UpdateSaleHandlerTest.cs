using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.Application.Interfaces;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;

using AutoMapper;

using Bogus;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Rebus.Bus;

using Xunit;

namespace Ambev.DeveloperEvaluation.Tests.Application.Sales
{
    public class UpdateSaleHandlerTests
    {
        private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();
        private readonly IBus _rebusBus = Substitute.For<IBus>();
        private readonly ILogger<UpdateSaleHandler> _logger = Substitute.For<ILogger<UpdateSaleHandler>>();
        private readonly Faker _faker = new();

        private UpdateSaleHandler CreateHandler() =>
            new UpdateSaleHandler(_saleRepository, _unitOfWork, _mapper, _rebusBus, _logger);

        [Fact]
        public async Task Handle_ShouldUpdateSaleAndReturnDto_WhenSaleExists()
        {
            // Arrange
            var saleId = Guid.NewGuid();
            var customerName = _faker.Name.FullName();
            var branchName = _faker.Company.CompanyName();

            var sale = Substitute.For<Sale>();
            _saleRepository.GetByIdAsync(saleId).Returns(sale);

            var expectedDto = new SaleDto { Id = saleId };
            _mapper.Map<SaleDto>(sale).Returns(expectedDto);

            var command = new UpdateSaleCommand(saleId, customerName, branchName);
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            sale.Received(1).UpdateCustomerDetails(customerName, null);
            sale.Received(1).UpdateBranchDetails(branchName, null);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            result.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenSaleNotFound()
        {
            // Arrange
            var saleId = Guid.NewGuid();
            _saleRepository.GetByIdAsync(saleId).Returns((Sale)null);

            var command = new UpdateSaleCommand(saleId, _faker.Name.FullName(), _faker.Company.CompanyName());
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
