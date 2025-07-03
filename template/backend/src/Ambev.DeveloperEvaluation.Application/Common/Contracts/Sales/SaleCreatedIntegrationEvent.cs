
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;

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

        public SaleCreatedIntegrationEvent(Sale sale)
        {
            SaleId = sale.Id;
            SaleNumber = sale.SaleNumber;
            SaleDate = sale.SaleDate;
            CustomerId = sale.CustomerId;
            CustomerName = sale.CustomerName;
            BranchId = sale.BranchId;
            BranchName = sale.BranchName;
            TotalAmount = sale.TotalAmount;
            Status = sale.Status.ToString();

            Items = sale.Items.Select(item => new SaleItemCreatedIntegrationEventDto
            {
                SaleItemId = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                ItemTotalAmount = item.ItemTotalAmount
            }).ToList();
        }
    }
}
