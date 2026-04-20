using OIG.Application;
using OIG.Application.Abstractions.Auth;
using OIG.Infrastructure;
using OIG.Infrastructure.Persistence;
using OIG.WebApi.Auth;
using OIG.WebApi.Endpoints;
using OIG.WebApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedDevelopmentData(db);

    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}
else
{
    app.UseHsts();
    app.MapGet("/", () => Results.Ok("OK")).ExcludeFromDescription();
}

app.UseHttpsRedirection();

app.MapApiEndpoints();

app.Run();

public partial class Program
{
    static void SeedDevelopmentData(AppDbContext db)
    {
        var existingUser = db.Set<OIG.Domain.Users.User>().FirstOrDefault();
        if (existingUser is not null)
        {
            Console.WriteLine($"[Seed] Existing user found: {existingUser.Name} -> {existingUser.Id.Value}");
            return;
        }

        var org = OIG.Domain.Organizations.Organization.CreateRoot("Root Organization");
        db.Set<OIG.Domain.Organizations.Organization>().Add(org);

        var role = OIG.Domain.Roles.Role.Create("Admin", org.Id.Value);
        foreach (var p in Enum.GetValues<OIG.Domain.Roles.Permission>())
            role.Grant(p);
        db.Set<OIG.Domain.Roles.Role>().Add(role);

        var admin = OIG.Domain.Users.User.Register("Admin", "admin@oig.local", org.Id.Value);
        admin.AssignRole(role.Id);
        db.Set<OIG.Domain.Users.User>().Add(admin);

        db.SaveChanges();

        Console.WriteLine($"[Seed] Admin user created: {admin.Id.Value}");
    }
}