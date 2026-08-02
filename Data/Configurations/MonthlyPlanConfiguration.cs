using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketFlow.Models;

namespace PocketFlow.Data.Configurations;

public class MonthlyPlanConfiguration : IEntityTypeConfiguration<MonthlyPlan>
{
    public void Configure(EntityTypeBuilder<MonthlyPlan> builder)
    {
        builder.HasKey(x => x.Id);

        // Unique index per account, month, year
        builder.HasIndex(x => new { x.AccountId, x.Month, x.Year })
            .IsUnique();

        builder.HasOne(x => x.BasedOnPlan)
            .WithMany()
            .HasForeignKey(x => x.BasedOnPlanId)
            .OnDelete(DeleteBehavior.SetNull); // Optional self-relation

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Income).HasPrecision(18, 2);
        builder.Property(x => x.FixedExpenses).HasPrecision(18, 2);
        builder.Property(x => x.TotalSavings).HasPrecision(18, 2);
        builder.Property(x => x.FreePocketAmount).HasPrecision(18, 2);
        builder.Property(x => x.WeeklyBudget).HasPrecision(18, 2);
        builder.Property(x => x.LifeBudget).HasPrecision(18, 2);
        builder.Property(x => x.WhimBudget).HasPrecision(18, 2);

        builder.HasMany(x => x.Expenses)
            .WithOne(e => e.MonthlyPlan)
            .HasForeignKey(e => e.MonthlyPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
