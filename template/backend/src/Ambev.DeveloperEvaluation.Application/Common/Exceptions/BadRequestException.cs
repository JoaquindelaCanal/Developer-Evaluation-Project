using System.Net;

namespace Ambev.DeveloperEvaluation.Application.Common.Exceptions
{
    /// <summary>
    /// Represents an HTTP 400 Bad Request exception.
    /// </summary>
    public class BadRequestException : Exception
    {
        public HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

        public BadRequestException()
            : base("Bad Request.") { }

        public BadRequestException(string message)
            : base(message) { }

        public BadRequestException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
