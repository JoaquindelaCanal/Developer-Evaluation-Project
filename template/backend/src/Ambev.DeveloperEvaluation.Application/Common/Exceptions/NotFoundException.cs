using System.Net;

namespace Ambev.DeveloperEvaluation.Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public HttpStatusCode StatusCode => HttpStatusCode.NotFound;

        public NotFoundException()
            : base("The requested resource was not found.") { }

        public NotFoundException(string message)
            : base(message) { }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
