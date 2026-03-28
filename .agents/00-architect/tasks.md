# Architect Agent - Tasks

> Execute in order. Output every file with full content.

---

1. Review existing `Ecommerce.AppHost` and `Ecommerce.ServiceDefaults`
2. Generate or adjust `.editorconfig`
3. Generate or adjust `Directory.Build.props`
4. Generate or adjust `Directory.Packages.props` with pinned package versions
5. Generate missing `.csproj` files with correct references
6. Generate or adjust the solution file so all projects are included
7. Generate `Dockerfile` for each runnable service that does not already have one
8. Generate `docker-compose.yml` for infrastructure and services
9. Generate `prometheus.yml`
10. Generate `.github/workflows/ci.yml`
11. Generate `GlobalUsings.cs` where needed
12. Update existing Aspire projects only as needed to reference new runnable services

## Done When

- `dotnet restore` succeeds
- `dotnet build` succeeds with the generated project skeleton
- existing Aspire projects still build
- `docker compose config` is valid
- project references resolve correctly
