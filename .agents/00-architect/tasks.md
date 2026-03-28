# Architect Agent - Tasks

> Execute in order. Output every file with full content.

---

1. Generate `.gitignore` for .NET, Docker, and common IDE artifacts
2. Generate `.editorconfig` aligned with `coding-standards.md`
3. Generate `Directory.Build.props`
4. Generate `Directory.Packages.props` with pinned package versions
5. Generate all `.csproj` files with correct references
6. Generate `Ecommerce.sln` with all projects included
7. Generate `Dockerfile` for each runnable service
8. Generate `docker-compose.yml` for infrastructure and services
9. Generate `prometheus.yml`
10. Generate `.github/workflows/ci.yml`
11. Generate `GlobalUsings.cs` where needed

## Done When

- `dotnet restore` succeeds
- `dotnet build` succeeds with the generated project skeleton
- `docker compose config` is valid
- Project references resolve correctly
