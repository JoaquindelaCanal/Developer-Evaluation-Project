using Ambev.DeveloperEvaluation.Application.Interfaces;
using Ambev.DeveloperEvaluation.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly DefaultContext _context;

        public SaleRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Sale sale)
        {
            await _context.Sales.AddAsync(sale);
        }

        public async Task<Sale> GetByIdAsync(Guid id)
        {
            return await _context.Sales
                                 .Include(s => s.Items)
                                 .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Sale>> GetAllAsync(int pageNumber, int pageSize, string sortBy, string search, CancellationToken cancellationToken)
        {
            IQueryable<Sale> query = _context.Sales.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.SaleNumber.Contains(search) || s.CustomerName.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {             
                switch (sortBy.ToLowerInvariant())
                {
                    case "saledate":
                        query = query.OrderBy(s => s.SaleDate);
                        break;
                    case "totalamount":
                        query = query.OrderBy(s => s.TotalAmount);
                        break;
                    case "customername":
                        query = query.OrderBy(s => s.CustomerName);
                        break;
                    default:
                        query = query.OrderBy(s => s.SaleNumber);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(s => s.SaleNumber);
            }

            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return await query.ToListAsync(cancellationToken);
        }
    }
}