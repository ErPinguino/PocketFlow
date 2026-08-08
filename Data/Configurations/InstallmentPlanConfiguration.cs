using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketFlow.Models;

namespace PocketFlow.Data.Configurations;

public class InstallmentPlanConfiguration : IEntityTypeConfiguration<InstallmentPlan>
{
    public void Configure(EntityTypeBuilder<InstallmentPlan> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Description).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Provider).HasMaxLength(100);
        
        builder.Property(p => p.TotalAmount).HasPrecision(18, 2);
        builder.Property(p => p.BaseInstallmentAmount).HasPrecision(18, 2);
        
        // Relación: Un Account tiene muchos InstallmentPlans
        builder.HasOne(p => p.Account)
            .WithMany(a => a.InstallmentPlans)
            .HasForeignKey(p => p.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
