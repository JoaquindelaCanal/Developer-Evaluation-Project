
namespace Ambev.DeveloperEvaluation.Application.Common.Contracts.Sales
{
    public class SaleCreatedIntegrationEvent
    {
        public Guid SaleId { get; set; }
        public string SaleNumber { get; set; }
        public DateTime SaleDate { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid BranchId { get; set; }
        public string BranchName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public List<SaleItemCreatedIntegrationEventDto> Items { get; set; } = new();
    }
}
