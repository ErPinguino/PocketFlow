using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketFlow.Models;

namespace PocketFlow.Data.Configurations;

public class PiggyBankConfiguration : IEntityTypeConfiguration<PiggyBank>
{
    public void Configure(EntityTypeBuilder<PiggyBank> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Icon)
            .HasMaxLength(10);

        builder.Property(x => x.TargetAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.CurrentAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.MonthlyContribution)
            .HasPrecision(18, 2);

        // Non-negative amounts
        builder.ToTable(t => {
            t.HasCheckConstraint("CK_PiggyBank_TargetAmount", "\"TargetAmount\" >= 0");
            t.HasCheckConstraint("CK_PiggyBank_CurrentAmount", "\"CurrentAmount\" >= 0");
            t.HasCheckConstraint("CK_PiggyBank_MonthlyContribution", "\"MonthlyContribution\" >= 0");
        });
        
        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);
    }
}
