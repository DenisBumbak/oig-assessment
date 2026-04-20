# OIG.CleanVSA

Scaffold project meeting requirements:
- REST (Minimal API)
- CQRS (MediatR)
- DDD rich-ish models (Domain contains behavior + invariants)
- Clean Architecture (App depends on Domain, Infra implements ports)
- Vertical Slice folders per feature
- No AutoMapper (manual DTO mapping)

## Run
Open `OIG.CleanVSA.sln`, set `OIG.WebApi` as startup, run and open `/swagger`.

SQLite database file: `oig.db` (created automatically).

## Optional: EF migrations (recommended)
From repo root:
```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project .\src\OIG.Infrastructure\OIG.Infrastructure.csproj --startup-project .\src\OIG.WebApi\OIG.WebApi.csproj --context OIG.Infrastructure.Persistence.AppDbContext --output-dir Persistence/Migrations
dotnet ef database update --project .\src\OIG.Infrastructure\OIG.Infrastructure.csproj --startup-project .\src\OIG.WebApi\OIG.WebApi.csproj --context OIG.Infrastructure.Persistence.AppDbContext
```
Then switch `EnsureCreated()` to `Migrate()` in `Program.cs`.
