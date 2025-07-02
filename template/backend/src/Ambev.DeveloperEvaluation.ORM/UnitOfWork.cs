using Ambev.DeveloperEvaluation.Application.Interfaces;

namespace Ambev.DeveloperEvaluation.ORM
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DefaultContext _context;

        public UnitOfWork(DefaultContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
