using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OIG.Domain.Organizations;
using OIG.Domain.Roles;
using OIG.Domain.Users;
using OIG.Infrastructure.Persistence;

namespace OIG.WebApi.IntegrationTests;

public sealed class TestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(),
        $"oig-integration-{Guid.NewGuid():N}.db");

    public Guid AdminUserId { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>(
                    "ConnectionStrings:DefaultConnection",
                    $"Data Source={_dbPath}")
            ]);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={_dbPath}"));
        });
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (AdminUserId == Guid.Empty)
        {
            var org = Organization.CreateRoot("TestOrg");
            db.Organizations.Add(org);

            var role = Role.Create("Admin", org.Id.Value);
            foreach (Permission p in Enum.GetValues<Permission>())
                if (p != Permission.None) role.Grant(p);
            db.Roles.Add(role);

            var user = User.Register("Admin", "admin@test.com", org.Id.Value);
            user.AssignRole(role.Id);
            db.Users.Add(user);

            db.SaveChanges();
            AdminUserId = user.Id.Value;
        }

        client.DefaultRequestHeaders.Add("X-User-Id", AdminUserId.ToString());
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        TryDeleteDb();
    }

    private void TryDeleteDb()
    {
        try
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }
        catch
        {
        }
    }
}
