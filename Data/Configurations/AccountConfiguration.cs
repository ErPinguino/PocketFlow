using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketFlow.Models;

namespace PocketFlow.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("EUR");

        // Payday between 1 and 31
        builder.ToTable(t => t.HasCheckConstraint("CK_Account_Payday", "\"Payday\" >= 1 AND \"Payday\" <= 31"));

        builder.Property(x => x.MonthlyIncome)
            .HasPrecision(18, 2);

        builder.HasMany(x => x.PiggyBanks)
            .WithOne(p => p.Account)
            .HasForeignKey(p => p.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.MonthlyPlans)
            .WithOne(m => m.Account)
            .HasForeignKey(m => m.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
