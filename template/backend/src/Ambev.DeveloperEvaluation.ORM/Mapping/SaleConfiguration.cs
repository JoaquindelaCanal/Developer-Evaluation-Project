using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.Infrastructure.ORM.Configuration
{
    public class SaleConfiguration : IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            builder.ToTable("Sales");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.SaleNumber)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(s => s.SaleDate)
                   .IsRequired();

            builder.Property(s => s.CustomerId)
                   .IsRequired();

            builder.Property(s => s.CustomerName)
                   .IsRequired()
                   .HasMaxLength(250);

            builder.Property(s => s.BranchId)
                   .IsRequired();

            builder.Property(s => s.BranchName)
                   .IsRequired()
                   .HasMaxLength(250);

            builder.Property(s => s.TotalAmount)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");

            builder.Property(s => s.Status)
                   .IsRequired()
                   .HasConversion<string>();
            
            builder.HasMany(s => s.Items)
                   .WithOne() 
                   .HasForeignKey(si => si.SaleId) 
                   .OnDelete(DeleteBehavior.Cascade);


            builder.Ignore(s => s.DomainEvents);

            
            builder.Property(s => s.CreatedAt).IsRequired();
            builder.Property(s => s.UpdatedAt).IsRequired(false);
        }
    }
}