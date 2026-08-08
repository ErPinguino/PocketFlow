using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketFlow.Models;

namespace PocketFlow.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.HasIndex(x => x.Email)
            .IsUnique();
            
        builder.Property(x => x.PasswordHash)
            .IsRequired(false);

        builder.Property(x => x.SupabaseUserId)
            .IsRequired(false)
            .HasMaxLength(255);

        builder.HasIndex(x => x.SupabaseUserId)
            .IsUnique();
            
        builder.HasOne(x => x.Account)
            .WithOne(a => a.User)
            .HasForeignKey<Account>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
