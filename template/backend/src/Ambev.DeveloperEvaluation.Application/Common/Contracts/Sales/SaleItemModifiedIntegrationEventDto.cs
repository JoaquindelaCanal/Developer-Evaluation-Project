using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Application.Common.Contracts.Sales
{
    public class SaleItemModifiedIntegrationEventDto
    {
        public Guid SaleItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal ItemTotalAmount { get; set; }
        public bool IsCancelled { get; set; }
    }
}
