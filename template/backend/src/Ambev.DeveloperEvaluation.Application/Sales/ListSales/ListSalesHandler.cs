using System.Globalization; 
using System.Linq.Dynamic.Core;
using System.Reflection;

using Ambev.DeveloperEvaluation.Application.Common.Exceptions;
using Ambev.DeveloperEvaluation.Application.Common.Models.QueryParameters;
using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.Application.Interfaces;

using AutoMapper;

using MediatR;


namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales
{
    public class ListSalesQueryHandler : IRequestHandler<ListSalesQuery, ApplicationPaginatedList<SaleDto>>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMapper _mapper;

        public ListSalesQueryHandler(ISaleRepository saleRepository, IMapper mapper)
        {
            _saleRepository = saleRepository;
            _mapper = mapper;
        }

        public async Task<ApplicationPaginatedList<SaleDto>> Handle(ListSalesQuery request, CancellationToken cancellationToken)
        {
            //initial IQueryable from the query service
            IQueryable<Domain.Entities.Sale> salesQuery = _saleRepository.GetQueryableSales();

            salesQuery = ApplyFilters(salesQuery, request.Filters);

            salesQuery = ApplySorting(salesQuery, request.SortOptions);

            var paginatedSalesFromDb = await ApplicationPaginatedList<Domain.Entities.Sale>.CreateAsync(salesQuery, request.Page, request.Size);

            //map the domain entities to SaleDto
            var salesDtos = _mapper.Map<IEnumerable<SaleDto>>(paginatedSalesFromDb.Items);

            // wrap the DTOs and pagination metadata into the ApplicationPaginatedList
            return new ApplicationPaginatedList<SaleDto>(
                salesDtos,
                paginatedSalesFromDb.TotalCount,
                paginatedSalesFromDb.CurrentPage,
                paginatedSalesFromDb.PageSize
            );
        }

        /// <summary>
        /// Dynamically applies filters to the IQueryable using System.Linq.Dynamic.Core.
        /// </summary>
        private IQueryable<Domain.Entities.Sale> ApplyFilters(IQueryable<Domain.Entities.Sale> query, Dictionary<string, List<FilterOption>> filters)
        {
            foreach (var kvp in filters)
            {
                string propertyName = kvp.Key;
                List<FilterOption> options = kvp.Value;

                if (options.Any())
                {
                    var predicates = new List<string>();
                    var parameters = new List<object>();
                    int paramIndex = 0;

                    // validate property existence before building predicate
                    PropertyInfo propInfo = typeof(Domain.Entities.Sale).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (propInfo == null)
                    {
                        throw new BadRequestException($"Invalid filter field: '{propertyName}'. Property does not exist on Sale entity.");
                    }

                    foreach (var option in options)
                    {
                        string currentPredicate = "";
                        object convertedValue = null;

                        if (option.Value != null)
                        {
                            try
                            {
                                Type targetType = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;
                                convertedValue = Convert.ChangeType(option.Value, targetType, CultureInfo.InvariantCulture);
                            }
                            catch (FormatException)
                            {
                                throw new BadRequestException($"Invalid format for filter field '{propertyName}'. Value '{option.Value}' cannot be converted to type '{propInfo.PropertyType.Name}'.");
                            }
                            catch (InvalidCastException)
                            {
                                throw new BadRequestException($"Cannot cast filter value '{option.Value}' to property type '{propInfo.PropertyType.Name}' for field '{propertyName}'.");
                            }
                        }

                        string valuePlaceholder = $"@{paramIndex++}";
                        parameters.Add(convertedValue);

                        // dynamic LINQ predicate string based on FilterOperation
                        switch (option.Operation)
                        {
                            case FilterOperation.Equals:
                                if (convertedValue == null)
                                {
                                    currentPredicate = $"{propertyName} == null";
                                    parameters.RemoveAt(parameters.Count - 1);
                                }
                                else
                                {
                                    currentPredicate = $"{propertyName} == {valuePlaceholder}";
                                }
                                break;
                            case FilterOperation.Contains:
                                if (propInfo.PropertyType == typeof(string))
                                    currentPredicate = $"{propertyName}.Contains({valuePlaceholder}, {(option.IsCaseInsensitive ? "StringComparison.OrdinalIgnoreCase" : "StringComparison.Ordinal")})";
                                else
                                    throw new BadRequestException($"'Contains' operation is only supported for string properties. Field: '{propertyName}'.");
                                break;
                            case FilterOperation.StartsWith:
                                if (propInfo.PropertyType == typeof(string))
                                    currentPredicate = $"{propertyName}.StartsWith({valuePlaceholder}, {(option.IsCaseInsensitive ? "StringComparison.OrdinalIgnoreCase" : "StringComparison.Ordinal")})";
                                else
                                    throw new BadRequestException($"'StartsWith' operation is only supported for string properties. Field: '{propertyName}'.");
                                break;
                            case FilterOperation.EndsWith:
                                if (propInfo.PropertyType == typeof(string))
                                    currentPredicate = $"{propertyName}.EndsWith({valuePlaceholder}, {(option.IsCaseInsensitive ? "StringComparison.OrdinalIgnoreCase" : "StringComparison.Ordinal")})";
                                else
                                    throw new BadRequestException($"'EndsWith' operation is only supported for string properties. Field: '{propertyName}'.");
                                break;
                            case FilterOperation.GreaterThan:
                                currentPredicate = $"{propertyName} > {valuePlaceholder}";
                                break;
                            case FilterOperation.LessThan:
                                currentPredicate = $"{propertyName} < {valuePlaceholder}";
                                break;
                            case FilterOperation.GreaterThanOrEqual:
                                currentPredicate = $"{propertyName} >= {valuePlaceholder}";
                                break;
                            case FilterOperation.LessThanOrEqual:
                                currentPredicate = $"{propertyName} <= {valuePlaceholder}";
                                break;
                            default:
                                throw new BadRequestException($"Unsupported filter operation: {option.Operation} for field '{propertyName}'.");
                        }
                        predicates.Add(currentPredicate);
                    }

                    if (predicates.Any())
                    {
                        query = query.Where($"({string.Join(" or ", predicates)})", parameters.ToArray());
                    }
                }
            }
            return query;
        }

        /// <summary>
        /// Dynamically applies sorting to the IQueryable using System.Linq.Dynamic.Core.
        /// </summary>
        private IQueryable<Domain.Entities.Sale> ApplySorting(IQueryable<Domain.Entities.Sale> query, List<SortOption> sortOptions)
        {
            if (!sortOptions.Any())
            {
                return query.OrderByDescending(s => s.SaleDate);
            }

            string orderByString = "";
            bool firstSort = true;

            foreach (var option in sortOptions)
            {
                // validate property existence before trying to sort
                PropertyInfo propInfo = typeof(Domain.Entities.Sale).GetProperty(option.FieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propInfo == null)
                {
                    throw new BadRequestException($"Invalid sort field: '{option.FieldName}'. Property does not exist on Sale entity.");
                }

                if (!firstSort)
                {
                    orderByString += ", "; // For subsequent 'ThenBy' in dynamic LINQ
                }

                orderByString += $"{option.FieldName} {option.Direction.ToString().ToLower()}";
                firstSort = false;
            }

            return query.OrderBy(orderByString);
        }
    }

}
