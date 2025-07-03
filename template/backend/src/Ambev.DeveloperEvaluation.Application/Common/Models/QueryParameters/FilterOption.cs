using Ambev.DeveloperEvaluation.Application.Common.Exceptions;

namespace Ambev.DeveloperEvaluation.Application.Common.Models.QueryParameters
{
    public class FilterOption
    {
        public string FieldName { get; }
        public FilterOperation Operation { get; }
        public string Value { get; }
        public bool IsCaseInsensitive { get; }

        public FilterOption(string fieldName, FilterOperation operation, string value, bool isCaseInsensitive = true)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                throw new BadRequestException("Filter field name cannot be empty.");
            }
            FieldName = fieldName;
            Operation = operation;
            Value = value;
            IsCaseInsensitive = isCaseInsensitive;
        }

        /// <summary>
        /// creates a FilterOption based on a query key and value, inferring the operation.
        /// </summary>
        /// <param name="queryKey">The query parameter key (e.g., "name", "_minPrice").</param>
        /// <param name="queryValue">The query parameter value (e.g., "john", "*smith", "100").</param>
        /// <returns>A FilterOption object.</returns>
        /// <exception cref="BadRequestException">Thrown if the key or value is invalid.</exception>
        public static FilterOption Create(string queryKey, string queryValue)
        {
            if (string.IsNullOrWhiteSpace(queryKey))
            {
                throw new BadRequestException("Invalid filter key.");
            }

            string fieldName = queryKey;
            FilterOperation operation = FilterOperation.Equals;
            string value = queryValue;
            bool isCaseInsensitive = true; 

            if (queryKey.StartsWith("_min", StringComparison.OrdinalIgnoreCase))
            {
                fieldName = queryKey.Substring(4);
                if (string.IsNullOrWhiteSpace(fieldName)) throw new BadRequestException("Missing field name for _min filter.");
                operation = FilterOperation.GreaterThanOrEqual; // _minPrice=50 implies Price >= 50
            }
            else if (queryKey.StartsWith("_max", StringComparison.OrdinalIgnoreCase))
            {
                fieldName = queryKey.Substring(4);
                if (string.IsNullOrWhiteSpace(fieldName)) throw new BadRequestException("Missing field name for _max filter.");
                operation = FilterOperation.LessThanOrEqual; // _maxPrice=200 implies Price <= 200
            }

            else if (value.StartsWith("*") && value.EndsWith("*") && value.Length > 1)
            {
                operation = FilterOperation.Contains;
                value = value.Trim('*'); // remove leading/trailing asterisks
            }
            else if (value.EndsWith("*") && value.Length > 1)
            {
                operation = FilterOperation.StartsWith;
                value = value.TrimEnd('*'); // remove trailing asterisk
            }
            else if (value.StartsWith("*") && value.Length > 1)
            {
                operation = FilterOperation.EndsWith;
                value = value.TrimStart('*'); // remove leading asterisk
            }

            return new FilterOption(fieldName, operation, value, isCaseInsensitive);
        }
    }

}