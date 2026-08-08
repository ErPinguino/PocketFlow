using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketFlow.Models;

namespace PocketFlow.Data.Configurations;

public class InstallmentPaymentConfiguration : IEntityTypeConfiguration<InstallmentPayment>
{
    public void Configure(EntityTypeBuilder<InstallmentPayment> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        
        // Relaciones
        builder.HasOne(p => p.InstallmentPlan)
            .WithMany(ip => ip.Payments)
            .HasForeignKey(p => p.InstallmentPlanId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(p => p.Expense)
            .WithMany()
            .HasForeignKey(p => p.ExpenseId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Índice único para asegurar que no se duplique la misma cuota
        builder.HasIndex(p => new { p.InstallmentPlanId, p.InstallmentNumber })
            .IsUnique();
    }
}
