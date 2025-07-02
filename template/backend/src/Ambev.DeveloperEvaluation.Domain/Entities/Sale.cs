using System;
using System.Collections.Generic;
using System.Linq;

using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums; 
using Ambev.DeveloperEvaluation.Domain.Events.Sales;

namespace Ambev.DeveloperEvaluation.Domain.Entities
{
    public class Sale : BaseEntity
    {
        public string SaleNumber { get; private set; }
        public DateTime SaleDate { get; private set; }

        public Guid CustomerId { get; private set; }
        public string CustomerName { get; private set; }

        public Guid BranchId { get; private set; }
        public string BranchName { get; private set; }

        public decimal TotalAmount { get; private set; }
        public SaleStatus Status { get; private set; }

        private readonly List<SaleItem> _items = new();
        public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

        // Private constructor for EF Core.
        private Sale() : base() { }

        public Sale(Guid customerId, string customerName, Guid branchId, string branchName, string saleNumber = null) : base()
        {
            SaleNumber = saleNumber ?? GenerateSaleNumber();
            SaleDate = DateTime.UtcNow;
            CustomerId = customerId;
            CustomerName = customerName;
            BranchId = branchId;
            BranchName = branchName;
            TotalAmount = 0; // Initial amount
            Status = SaleStatus.Active;

            // Add a Domain Event for Sale Creation
            AddDomainEvent(new SaleCreatedEvent(Id, SaleNumber, CustomerId, CustomerName, BranchId, BranchName, SaleDate));
        }


        public void AddItem(SaleItem item)
        {
            if (Status == SaleStatus.Cancelled)
            {
                throw new InvalidOperationException("Cannot add items to a cancelled sale.");
            }
            if (_items.Any(i => i.Id == item.Id)) // Prevent adding the exact same SaleItem instance twice
            {
                throw new InvalidOperationException($"Sale item with ID {item.Id} already exists in this sale.");
            }

            // Link the item to this sale aggregate
            item.SetSaleId(Id);

            _items.Add(item);
            RecalculateTotal();

            // Add a Domain Event for Item Added
            AddDomainEvent(new SaleItemAddedEvent(Id, item.Id, item.ProductId, item.Quantity, item.UnitPrice, item.DiscountPercentage));
        }

        public void CancelItem(Guid itemId)
        {
            if (Status == SaleStatus.Cancelled)
            {
                throw new InvalidOperationException("Cannot cancel items in a cancelled sale.");
            }

            var itemToCancel = _items.FirstOrDefault(i => i.Id == itemId);
            if (itemToCancel == null)
            {
                throw new InvalidOperationException($"Sale item with ID {itemId} not found in this sale.");
            }

            if (itemToCancel.IsCancelled)
            {
                throw new InvalidOperationException($"Sale item with ID {itemId} is already cancelled.");
            }

            itemToCancel.Cancel(); // Delegate cancellation to the SaleItem itself
            RecalculateTotal();

            // Add a Domain Event for Item Cancelled
            AddDomainEvent(new SaleItemCancelledEvent(Id, itemId));
        }

        public void RecalculateTotal()
        {
            TotalAmount = _items.Where(item => !item.IsCancelled).Sum(item => item.ItemTotalAmount);
        }

        public void Cancel()
        {
            if (Status == SaleStatus.Cancelled)
            {
                throw new InvalidOperationException("Sale is already cancelled.");
            }

            Status = SaleStatus.Cancelled;

            // Also cancel all active items in the sale when the sale itself is cancelled
            foreach (var item in _items.Where(i => !i.IsCancelled))
            {
                item.Cancel();
            }
            RecalculateTotal();

            // Add a Domain Event for Sale Cancelled
            AddDomainEvent(new SaleCancelledEvent(Id, SaleNumber, DateTime.UtcNow));
        }

        public void Complete()
        {
            if (Status != SaleStatus.Active)
            {
                throw new InvalidOperationException("Only active sales can be completed.");
            }
            Status = SaleStatus.Completed;
            AddDomainEvent(new SaleCompletedEvent(Id, SaleNumber, DateTime.UtcNow));
        }


        // Helper to generate a unique sale number
        private string GenerateSaleNumber()
        {
            return $"SALE-{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }
    }
}