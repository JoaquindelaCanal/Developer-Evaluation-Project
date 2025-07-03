using Ambev.DeveloperEvaluation.Application.Common.Models.QueryParameters;
using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.WebApi.Common;

using AutoMapper;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public SalesController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves a paginated and filterable list of sales.
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="size">Page size (default: 10)</param>
        /// <param name="sort">Sorting options (e.g., "SaleDate desc", "ClientName asc")</param>
        /// <param name="filter">Filtering options (e.g., "ClientName=John*", "_minTotalAmount=100")</param>
        /// <returns>A paginated response of SaleDto.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<SaleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ListSales(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery(Name = "sort")] List<string> sort = null,
            [FromQuery(Name = "filter")] Dictionary<string, List<string>> filter = null)
        {
            var query = new ListSalesQuery
            {
                Page = page,
                Size = size,
                SortOptions = new List<SortOption>(),
                Filters = new Dictionary<string, List<FilterOption>>()
            };

            if (sort != null && sort.Any())
            {
                foreach (var s in sort)
                {
                    try
                    {
                        query.SortOptions.Add(SortOption.Parse(s));
                    }
                    catch (Application.Common.Exceptions.BadRequestException ex)
                    {
                        return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
                    }
                }
            }

            if (filter != null && filter.Any())
            {
                foreach (var kvp in filter)
                {
                    string filterKey = kvp.Key;
                    List<string> filterValues = kvp.Value;

                    query.Filters[filterKey] = new List<FilterOption>();
                    foreach (var val in filterValues)
                    {
                        try
                        {
                            query.Filters[filterKey].Add(FilterOption.Create(filterKey, val));
                        }
                        catch (Application.Common.Exceptions.BadRequestException ex)
                        {
                            return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
                        }
                    }
                }
            }

            ApplicationPaginatedList<SaleDto> applicationResponse;

            try
            {
                applicationResponse = await _mediator.Send(query);
            }
            catch (Application.Common.Exceptions.BadRequestException ex) //validation errors from Application layer
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Success = false, Message = "An unexpected error occurred." });
            }

            var webApiResponse = _mapper.Map<PaginatedResponse<SaleDto>>(applicationResponse);

            return Ok(webApiResponse);
        }
    }
}
