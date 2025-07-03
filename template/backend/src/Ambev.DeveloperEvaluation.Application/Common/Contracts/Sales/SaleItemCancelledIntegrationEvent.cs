using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Application.Common.Contracts.Sales
{
    public class SaleItemCancelledIntegrationEvent
    {
        public Guid SaleId { get; set; }
        public Guid SaleItemId { get; set; }
        public Guid ProductId { get; set; }
        public DateTime CancellationDate { get; set; }
    }
}
