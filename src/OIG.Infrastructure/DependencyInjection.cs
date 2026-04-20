using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OIG.Application.Abstractions.Auth;
using OIG.Application.Abstractions.Persistence;
using OIG.Infrastructure.Auth;
using OIG.Infrastructure.Persistence;
using OIG.Infrastructure.Persistence.Repositories;

namespace OIG.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=oig.db";
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(conn));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPermissionService, PermissionService>();

        return services;
    }
}
