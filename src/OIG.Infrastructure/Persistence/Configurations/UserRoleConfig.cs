using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OIG.Domain.Roles;
using OIG.Domain.Users;

namespace OIG.Infrastructure.Persistence.Configurations;

public sealed class UserRoleConfig : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> b)
    {
        b.ToTable("UserRoles");
        b.HasKey(x => x.Id);

        b.Property(x => x.UserId)
            .HasConversion(v => v.Value, v => new UserId(v))
            .IsRequired();

        b.Property(x => x.RoleId)
            .HasConversion(v => v.Value, v => new RoleId(v))
            .IsRequired();

        b.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
    }
}
