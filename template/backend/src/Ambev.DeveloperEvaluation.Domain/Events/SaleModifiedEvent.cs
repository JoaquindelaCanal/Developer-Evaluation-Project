using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events
{
    public record SaleModifiedEvent(
            Guid SaleId,
            string OldSaleNumber,
            string NewSaleNumber,
            string OldCustomerName,
            string NewCustomerName,
            string OldBranchName,
            string NewBranchName,
            DateTime ModifiedDate,          
            List<string> ChangedProperties = null
        ) : INotification;
}
