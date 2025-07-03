

namespace Ambev.DeveloperEvaluation.Application.Common.Contracts.Sales
{
    public class SaleCancelledIntegrationEvent
    {
        public Guid SaleId { get; set; }
        public string SaleNumber { get; set; }
        public DateTime CancellationDate { get; set; }
        public Guid? CancelledByUserId { get; set; }
    }
}
