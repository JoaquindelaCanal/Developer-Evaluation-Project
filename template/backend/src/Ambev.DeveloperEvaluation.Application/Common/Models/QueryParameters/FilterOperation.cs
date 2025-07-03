namespace Ambev.DeveloperEvaluation.Application.Common.Models.QueryParameters
{
    public enum FilterOperation
    {
        /// <summary>
        /// Exact match (e.g., field=value)
        /// </summary>
        Equals,
        /// <summary>
        /// String contains (e.g., field=*value* or field=value* or field=*value)
        /// </summary>
        Contains,
        /// <summary>
        /// String starts with (e.g., field=value*)
        /// </summary>
        StartsWith,
        /// <summary>
        /// String ends with (e.g., field=*value)
        /// </summary>
        EndsWith,
        /// <summary>
        /// Numeric/Date greater than (e.g., _minField=value)
        /// </summary>
        GreaterThan,
        /// <summary>
        /// Numeric/Date less than (e.g., _maxField=value)
        /// </summary>
        LessThan,
        /// <summary>
        /// Numeric/Date greater than or equal (e.g., _minField=value)
        /// </summary>
        GreaterThanOrEqual,
        /// <summary>
        /// Numeric/Date less than or equal (e.g., _maxField=value)
        /// </summary>
        LessThanOrEqual
    }
}
