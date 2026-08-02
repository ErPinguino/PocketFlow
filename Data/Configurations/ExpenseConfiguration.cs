using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketFlow.Models;

namespace PocketFlow.Data.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.ToTable(t => t.HasCheckConstraint("CK_Expense_Amount", "\"Amount\" > 0"));

        builder.Property(x => x.Category)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Description)
            .HasMaxLength(500);
    }
}
