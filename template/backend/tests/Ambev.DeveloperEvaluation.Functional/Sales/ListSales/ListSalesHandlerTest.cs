using Ambev.DeveloperEvaluation.Application.Common.Exceptions;
using Ambev.DeveloperEvaluation.Application.Common.Models.QueryParameters;
using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.Application.Interfaces;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Domain.Entities;

using AutoMapper;

using Bogus;

using FluentAssertions;

using NSubstitute;

using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Sales.ListSales
{
    public class ListSalesQueryHandlerTests
    {
        private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();
        private readonly Faker _faker = new();

        private ListSalesQueryHandler CreateHandler() =>
            new ListSalesQueryHandler(_saleRepository, _mapper);

        private IQueryable<Sale> GetFakeSalesQueryable(int count = 10)
        {
            var sales = new Faker<Sale>()
                .CustomInstantiator(f => new Sale(
                    f.Random.Guid(),
                    f.Name.FullName(),
                    f.Random.Guid(),
                    f.Company.CompanyName(),
                    f.Random.Int(1000, 9999).ToString()))
                .Generate(count)
                .AsQueryable();

            return sales;
        }

        [Fact]
        public async Task Handle_ShouldReturnPaginatedSaleDtos_WhenRequestIsValid()
        {
            // Arrange
            var fakeSales = GetFakeSalesQueryable();
            var fakeDtos = fakeSales.Select(s => new SaleDto { Id = s.Id }).ToList();

            var paginatedResult = new ApplicationPaginatedList<Sale>(
                fakeSales.Take(5), fakeSales.Count(), 1, 5);

            _saleRepository.GetQueryableSales().Returns(fakeSales);

            _mapper.Map<IEnumerable<SaleDto>>(Arg.Any<IEnumerable<Sale>>())
                .Returns(fakeDtos);

            var query = new ListSalesQuery { Page = 1, Size = 5 };

            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(fakeDtos.Count);
            result.TotalCount.Should().Be(fakeSales.Count());
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenFilterFieldDoesNotExist()
        {
            // Arrange
            var query = new ListSalesQuery
            {
                Page = 1,
                Size = 10,
                Filters = new Dictionary<string, List<FilterOption>>
                {
                    ["InvalidField"] = new List<FilterOption>
                    {
                        new FilterOption(null, FilterOperation.Equals,  "x")
                    }
                }
            };

            _saleRepository.GetQueryableSales().Returns(GetFakeSalesQueryable());

            var handler = CreateHandler();

            // Act
            Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("*Invalid filter field*");
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenSortFieldDoesNotExist()
        {
            // Arrange
            var query = new ListSalesQuery
            {
                Page = 1,
                Size = 10,
                SortOptions = new List<SortOption>
                {
                    new SortOption("NonExistingProp", SortDirection.Asc)
                }
            };

            _saleRepository.GetQueryableSales().Returns(GetFakeSalesQueryable());

            var handler = CreateHandler();

            // Act
            Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("*Invalid sort field*");
        }
    }
}
