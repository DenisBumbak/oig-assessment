using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OIG.Domain.Users;

namespace OIG.Infrastructure.Persistence.Configurations;

public sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasConversion(v => v.Value, v => new UserId(v))
            .ValueGeneratedNever();

        b.Property(x => x.Name).HasMaxLength(200).IsRequired();

        b.OwnsOne(x => x.Email, eb =>
        {
            eb.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(320)
                .IsRequired();

            eb.HasIndex(e => e.Value).IsUnique();
        });

        b.Property(x => x.IsActive).IsRequired();
        b.Property(x => x.OrganizationId);

        b.HasMany(u => (ICollection<UserRole>)u.UserRoles)
            .WithOne()
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
