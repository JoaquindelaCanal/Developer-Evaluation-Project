using Ambev.DeveloperEvaluation.Application.Common.Exceptions;
using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.Application.Interfaces;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;

using AutoMapper;

using Bogus;

using Castle.Core.Resource;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Rebus.Bus;

using Xunit;
using Xunit.Abstractions;



namespace Ambev.DeveloperEvaluation.Functional.Sales.CreateSale
{
    public class CreateSaleHandlerTests
    {
        private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();
        private readonly IBus _rebus = Substitute.For<IBus>();
        private readonly ILogger<CreateSaleHandler> _logger = Substitute.For<ILogger<CreateSaleHandler>>();
        private readonly Faker _faker = new();

        private CreateSaleHandler CreateHandler() =>
            new CreateSaleHandler(_saleRepository, _unitOfWork, _mapper, _rebus, _logger);


        private CreateSaleCommand ValidCommand() => new(
           Guid.NewGuid(),
           _faker.Name.FullName(),
           Guid.NewGuid(),
           _faker.Company.CompanyName(),
           _faker.Random.Int(1000, 9999).ToString(),
            new List<SaleItemDto>
            {
                new SaleItemDto
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = _faker.Commerce.ProductName(),
                    Quantity = 2,
                    UnitPrice = 20
                }
            });

        private CreateSaleCommand InValidCommandName() => new(
           Guid.NewGuid(),
           " ",
           Guid.NewGuid(),
           _faker.Company.CompanyName(),
           _faker.Random.Int(1000, 9999).ToString(),
            new List<SaleItemDto>
            {
                new SaleItemDto
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = _faker.Commerce.ProductName(),
                    Quantity = 2,
                    UnitPrice = 20
                }
            });

        private CreateSaleCommand InValidCommandItems() => new(
            Guid.NewGuid(),
            " ",
            Guid.NewGuid(),
            _faker.Company.CompanyName(),
            _faker.Random.Int(1000, 9999).ToString(),
             null);

        [Fact]
        public async Task Handle_ShouldReturnSaleDto_WhenRequestIsValid()
        {
            // Arrange
            var command = ValidCommand();
            var sale = new Sale(command.CustomerId, command.CustomerName, command.BranchId, command.BranchName, command.SaleNumber);
            var expectedDto = new SaleDto { Id = Guid.NewGuid() };

            _mapper.Map<SaleDto>(Arg.Any<Sale>()).Returns(expectedDto);

            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedDto);
            await _saleRepository.Received(1).AddAsync(Arg.Any<Sale>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequest_WhenCustomerNameIsEmpty()
        {
            var command = InValidCommandName();
            
            var handler = CreateHandler();

            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("Customer name is required.");
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequest_WhenItemsIsNull()
        {
            var command = InValidCommandItems();

            var handler = CreateHandler();

            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("Sale must have at least one item.");
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequest_WhenQuantityIsLessThanOrEqualToZero()
        {
            var command = ValidCommand();
            command.Items.First().Quantity = 0;

            var handler = CreateHandler();

            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage($"Quantity for product {command.Items.First().ProductId} must be greater than zero.");
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequest_WhenUnitPriceIsLessThanOrEqualToZero()
        {
            var command = ValidCommand();
            command.Items.First().UnitPrice = 0;

            var handler = CreateHandler();

            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage($"Unit price for product {command.Items.First().ProductId} must be greater than zero.");
        }
    }
}
