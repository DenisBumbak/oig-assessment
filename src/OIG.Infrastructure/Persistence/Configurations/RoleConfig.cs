using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OIG.Domain.Roles;

namespace OIG.Infrastructure.Persistence.Configurations;

public sealed class RoleConfig : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("Roles");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasConversion(v => v.Value, v => new RoleId(v))
            .ValueGeneratedNever();

        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.OrganizationId);

        b.HasMany(r => (ICollection<RolePermission>)r.Permissions)
            .WithOne()
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
