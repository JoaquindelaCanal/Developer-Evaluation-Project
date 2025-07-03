using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.Application.Common.Models.QueryParameters
{
    /// <summary>
    /// paginated list of items at the application layer, independent of API-specific response formats.
    /// </summary>
    /// <typeparam name="T">The type of items in the list.</typeparam>
    public class ApplicationPaginatedList<T> : List<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }

        public ApplicationPaginatedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            AddRange(items);
        }

        public static async Task<ApplicationPaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            return new ApplicationPaginatedList<T>(items, count, pageNumber, pageSize);
        }

        public ApplicationPaginatedList() { }
    }
}
