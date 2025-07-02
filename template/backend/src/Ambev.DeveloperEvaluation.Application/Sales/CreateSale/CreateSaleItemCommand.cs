namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale
{
    public record CreateSaleItemCommand(
        Guid ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice
    );
}
