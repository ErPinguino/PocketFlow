using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketFlow.Models;

namespace PocketFlow.Data.Configurations;

public class PiggyBankContributionConfiguration : IEntityTypeConfiguration<PiggyBankContribution>
{
    public void Configure(EntityTypeBuilder<PiggyBankContribution> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.ToTable(t => t.HasCheckConstraint("CK_PiggyBankContribution_Amount", "\"Amount\" > 0"));

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(x => x.PiggyBank)
            .WithMany()
            .HasForeignKey(x => x.PiggyBankId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MonthlyPlan)
            .WithMany()
            .HasForeignKey(x => x.MonthlyPlanId)
            .OnDelete(DeleteBehavior.Restrict); // Maintain history even if we do something with plans? Wait, plans cascade from Account. If plan deleted, contributions deleted is fine. But usually we don't delete plans. We will use Cascade for plan as well, but wait, if we delete a plan, we don't want to lose piggybank total? If plan deleted, contributions are rolled back? Actually, we'll use Restrict to be safe. Wait, the user asked to preserve history. So Cascade is probably fine for Account -> Plan -> Contribution.
            
        // Índice sugerido por el usuario
        builder.HasIndex(x => new { x.PiggyBankId, x.MonthlyPlanId, x.Type });
    }
}
