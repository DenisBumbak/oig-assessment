using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OIG.Domain.Roles;

namespace OIG.Infrastructure.Persistence.Configurations;

public sealed class RolePermissionConfig : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> b)
    {
        b.ToTable("RolePermissions");
        b.HasKey(x => x.Id);

        b.Property(x => x.RoleId)
            .HasConversion(v => v.Value, v => new RoleId(v))
            .IsRequired();

        b.Property(x => x.Permission)
            .HasConversion<int>()
            .IsRequired();

        b.HasIndex(x => new { x.RoleId, x.Permission }).IsUnique();
    }
}
