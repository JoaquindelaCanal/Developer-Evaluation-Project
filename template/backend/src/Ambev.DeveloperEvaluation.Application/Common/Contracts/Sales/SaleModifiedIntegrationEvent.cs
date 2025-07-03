namespace Ambev.DeveloperEvaluation.Application.Common.Contracts.Sales
{
    public class SaleModifiedIntegrationEvent
    {
        public Guid SaleId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string NewCustomerName { get; set; }
        public string NewBranchName { get; set; }
        public decimal NewTotalAmount { get; set; }
        public string NewStatus { get; set; }
        public List<SaleItemModifiedIntegrationEventDto> UpdatedItems { get; set; } = new();
    }
}
