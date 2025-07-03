using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.Infrastructure.ORM.Configuration
{
    public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
    {
        public void Configure(EntityTypeBuilder<SaleItem> builder)
        {
            builder.ToTable("SaleItems");

            builder.HasKey(si => si.Id);

            builder.Property(si => si.SaleId)
                   .IsRequired();

            builder.Property(si => si.ProductId)
                   .IsRequired();

            builder.Property(si => si.ProductName)
                   .IsRequired()
                   .HasMaxLength(250);

            builder.Property(si => si.Quantity)
                   .IsRequired();

            builder.Property(si => si.UnitPrice)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");

            builder.Property(si => si.DiscountPercentage)
                   .IsRequired()
                   .HasColumnType("decimal(5,4)");

            builder.Property(si => si.DiscountAmount)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");

            builder.Property(si => si.ItemTotalAmount)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");

            builder.Property(si => si.IsCancelled)
                   .IsRequired();

            builder.Ignore(si => si.DomainEvents);

            builder.Property(si => si.CreatedAt).IsRequired();
            builder.Property(si => si.UpdatedAt).IsRequired(false);
        }
    }
}