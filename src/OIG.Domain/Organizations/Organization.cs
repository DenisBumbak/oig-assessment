using OIG.Domain.Common;

namespace OIG.Domain.Organizations;

public sealed class Organization : Entity<OrganizationId>
{
    private readonly List<Organization> _children = [];
    public string Name { get; private set; }
    public OrganizationId? ParentId { get; private set; }
    public IReadOnlyCollection<Organization> Children => _children;

    private Organization(OrganizationId id, string name, OrganizationId? parentId) : base(id)
    {
        Name = NormalizeName(name);
        ParentId = parentId;
    }

    // EF
    private Organization() : base(default) { Name = string.Empty; }

    public static Organization CreateRoot(string name) => new(OrganizationId.New(), name, null);

    public static Organization CreateChild(string name, Organization parent)
        => new(OrganizationId.New(), name, parent.Id);

    public void Rename(string name) => Name = NormalizeName(name);

    public void ChangeParent(OrganizationId? parentId) => ParentId = parentId;

    private static string NormalizeName(string name)
    {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Organization name is required.");
        if (name.Length > 200)
            throw new DomainException("Organization name is too long.");
        return name;
    }
}
