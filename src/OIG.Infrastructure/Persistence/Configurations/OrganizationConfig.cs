using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OIG.Domain.Organizations;

namespace OIG.Infrastructure.Persistence.Configurations;

public sealed class OrganizationConfig : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> b)
    {
        b.ToTable("Organizations");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasConversion(v => v.Value, v => new OrganizationId(v))
            .ValueGeneratedNever();

        b.Property(x => x.Name).HasMaxLength(200).IsRequired();

        b.Property(x => x.ParentId)
            .HasConversion(
                v => v.HasValue ? v.Value.Value : (Guid?)null,
                v => v.HasValue ? new OrganizationId(v.Value) : (OrganizationId?)null);

        b.HasMany(o => (ICollection<Organization>)o.Children)
            .WithOne()
            .HasForeignKey("ParentId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
