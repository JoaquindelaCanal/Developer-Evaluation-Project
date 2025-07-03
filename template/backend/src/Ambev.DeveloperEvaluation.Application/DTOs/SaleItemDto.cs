namespace Ambev.DeveloperEvaluation.Application.DTOs
{
    public class SaleItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }
        public bool IsCancelled { get; set; }
    }
}
