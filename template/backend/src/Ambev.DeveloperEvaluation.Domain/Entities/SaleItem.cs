using System;

using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums; 

namespace Ambev.DeveloperEvaluation.Domain.Entities.Sales
{
    public class SaleItem : BaseEntity
    {
        public Guid SaleId { get; private set; }

        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            private set
            {
                if (value > 20)
                {
                    throw new InvalidOperationException("Cannot sell more than 20 identical items in a single sale item entry.");
                }
                if (value <= 0)
                {
                    throw new InvalidOperationException("Sale item quantity must be positive.");
                }
                _quantity = value;
            }
        }

        public decimal UnitPrice { get; private set; }
        public decimal DiscountPercentage { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public decimal ItemTotalAmount { get; private set; }

        public bool IsCancelled { get; private set; }

        // Private constructor for EF Core and internal use
        private SaleItem() : base() { }

        public SaleItem(Guid productId, string productName, int quantity, decimal unitPrice) : base()
        {
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
            IsCancelled = false;

            ApplyDiscount();
        }

        public void SetSaleId(Guid saleId)
        {
            if (SaleId != Guid.Empty && SaleId != saleId)
            {
                // This prevents accidentally re-assigning an item to a different sale
                throw new InvalidOperationException("SaleItem is already associated with a different Sale.");
            }
            SaleId = saleId;
        }

        // Business rule: Apply discount based on quantity
        public void ApplyDiscount()
        {
            if (IsCancelled)
            {
                DiscountPercentage = 0;
                DiscountAmount = 0;
                ItemTotalAmount = 0;
                return;
            }

            // Reset discount for recalculation
            DiscountPercentage = 0;

            if (Quantity >= 10 && Quantity <= 20) // Purchases between 10 and 20 identical items have a 20% discount
            {
                DiscountPercentage = 0.20m;
            }
            else if (Quantity >= 4) // Purchases above 4 identical items have a 10% discount
            {
                DiscountPercentage = 0.10m; // 10%
            }

            DiscountAmount = (UnitPrice * Quantity) * DiscountPercentage;
            ItemTotalAmount = (UnitPrice * Quantity) - DiscountAmount;
        }

        public void Cancel()
        {
            if (IsCancelled)
            {
                throw new InvalidOperationException("Sale item is already cancelled.");
            }
            IsCancelled = true;
            ApplyDiscount();
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (IsCancelled)
            {
                throw new InvalidOperationException("Cannot update quantity of a cancelled sale item.");
            }
            Quantity = newQuantity;
            ApplyDiscount();
        }

        public void UpdateUnitPrice(decimal newUnitPrice)
        {
            if (IsCancelled)
            {
                throw new InvalidOperationException("Cannot update unit price of a cancelled sale item.");
            }
            if (newUnitPrice <= 0)
            {
                throw new InvalidOperationException("Unit price must be positive.");
            }
            UnitPrice = newUnitPrice;
            ApplyDiscount();
        }
    }
}