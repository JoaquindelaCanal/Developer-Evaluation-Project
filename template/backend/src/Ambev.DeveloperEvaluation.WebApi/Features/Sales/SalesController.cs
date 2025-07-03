using Ambev.DeveloperEvaluation.Application.Common.Exceptions;
using Ambev.DeveloperEvaluation.Application.Common.Models.QueryParameters;
using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSaleById;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
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

        /// <summary>
        /// Creates a new sale.
        /// </summary>
        /// <param name="command">The command containing sale data.</param>
        /// <returns>The ID of the newly created sale.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseWithData<Guid>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleCommand command)
        {
            if (command == null)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Request body cannot be empty." });
            }

            try
            {
                var newSale = await _mediator.Send(command);

                // 201 Created with the ID of the new resource
                return CreatedAtAction(nameof(ListSales), new { id = newSale.Id }, new ApiResponseWithData<Guid>
                {
                    Success = true,
                    Message = "Sale created successfully.",
                    Data = newSale.Id
                });
            }
            catch (Application.Common.Exceptions.BadRequestException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Success = false, Message = "An unexpected error occurred while creating the sale." });
            }
        }

        /// <summary>
        /// Retrieves a sale by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the sale to retrieve.</param>
        /// <returns>A SaleDto if found, otherwise 404 Not Found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponseWithData<SaleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSaleById(Guid id)
        {
            var query = new GetSaleByIdQuery(id);

            try
            {
                var saleDto = await _mediator.Send(query);
                return Ok(new ApiResponseWithData<SaleDto>
                {
                    Success = true,
                    Message = "Sale retrieved successfully.",
                    Data = saleDto
                });
            }
            catch (Application.Common.Exceptions.NotFoundException ex)
            {
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Success = false, Message = "An unexpected error occurred while retrieving the sale." });
            }
        }

        /// <summary>
        /// Updates an existing sale.
        /// </summary>
        /// <param name="id">The GUID of the sale to update.</param>
        /// <param name="command">The command containing updated sale data.</param>
        /// <returns>No content on success, or appropriate error response.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSale(Guid id, [FromBody] UpdateSaleCommand command)
        {
            if (command == null)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Request body cannot be empty." });
            }

            try
            {
                await _mediator.Send(command);

                return NoContent();
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (NotFoundException ex) 
            {
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Success = false, Message = "An unexpected error occurred while updating the sale." });
            }
        }

        
        /// <summary>
        /// Deletes a sale by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the sale to delete.</param>
        /// <returns>No content on success, or 404 Not Found.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteSale(Guid id)
        {
            var command = new CancelSaleCommand(id);
            try
            {
                await _mediator.Send(command);

                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Success = false, Message = "An unexpected error occurred while deleting the sale." });
            }
        }

    }
}
