using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketFlow.Models;

namespace PocketFlow.Data.Configurations;

public class MonthlyRolloverConfiguration : IEntityTypeConfiguration<MonthlyRollover>
{
    public void Configure(EntityTypeBuilder<MonthlyRollover> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.ToTable(t => t.HasCheckConstraint("CK_MonthlyRollover_Amount", "\"Amount\" > 0"));

        builder.Property(x => x.DestinationType)
            .HasConversion<string>()
            .HasMaxLength(30);

        // Relations
        builder.HasOne(x => x.FromMonthlyPlan)
            .WithMany()
            .HasForeignKey(x => x.FromMonthlyPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ToMonthlyPlan)
            .WithMany()
            .HasForeignKey(x => x.ToMonthlyPlanId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.PiggyBank)
            .WithMany()
            .HasForeignKey(x => x.PiggyBankId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
