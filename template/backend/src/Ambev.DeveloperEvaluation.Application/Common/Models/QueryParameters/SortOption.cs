using Ambev.DeveloperEvaluation.Application.Common.Exceptions;

namespace Ambev.DeveloperEvaluation.Application.Common.Models.QueryParameters
{
    public class SortOption
    {
        public string FieldName { get; }
        public SortDirection Direction { get; }

        public SortOption(string fieldName, SortDirection direction)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                throw new BadRequestException("sort field name cannot be empty.");
            }
            FieldName = fieldName;
            Direction = direction;
        }

        /// <summary>
        /// it parses a single sort string (e.g., "price desc", "name asc", or just "id").
        /// </summary>
        /// <param name="sortString">The string to parse.</param>
        /// <returns>A SortOption object.</returns>
        /// <exception cref="BadRequestException">Thrown if the format is invalid.</exception>
        public static SortOption Parse(string sortString)
        {
            if (string.IsNullOrWhiteSpace(sortString))
            {
                throw new BadRequestException("the sort string format is invalid expected 'field' or 'field asc/desc'.");
            }

            var parts = sortString.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string fieldName = parts[0];
            SortDirection direction = SortDirection.Asc;

            if (parts.Length > 1)
            {
                if (!Enum.TryParse(parts[1], true, out direction))
                {
                    throw new BadRequestException($"invalid sort direction '{parts[1]}' expected 'asc' or 'desc'.");
                }
            }

            return new SortOption(fieldName, direction);
        }
    }
}