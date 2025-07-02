using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Interfaces
{
    public interface ISaleRepository
    {
        Task AddAsync(Sale sale);
        Task<Sale> GetByIdAsync(Guid id);
        Task<IEnumerable<Sale>> GetAllAsync(int pageNumber, int pageSize, string sortBy, string search, CancellationToken cancellationToken);
    }
}