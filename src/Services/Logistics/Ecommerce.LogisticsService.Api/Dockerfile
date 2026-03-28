FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/SharedKernel/Ecommerce.Contracts/Ecommerce.Contracts.csproj", "src/SharedKernel/Ecommerce.Contracts/"]
COPY ["src/SharedKernel/Ecommerce.Common/Ecommerce.Common.csproj", "src/SharedKernel/Ecommerce.Common/"]
COPY ["src/Orchestration/Ecommerce.ServiceDefaults/Ecommerce.ServiceDefaults.csproj", "src/Orchestration/Ecommerce.ServiceDefaults/"]
COPY ["src/Services/Logistics/Ecommerce.LogisticsService.Domain/Ecommerce.LogisticsService.Domain.csproj", "src/Services/Logistics/Ecommerce.LogisticsService.Domain/"]
COPY ["src/Services/Logistics/Ecommerce.LogisticsService.Application/Ecommerce.LogisticsService.Application.csproj", "src/Services/Logistics/Ecommerce.LogisticsService.Application/"]
COPY ["src/Services/Logistics/Ecommerce.LogisticsService.Infrastructure/Ecommerce.LogisticsService.Infrastructure.csproj", "src/Services/Logistics/Ecommerce.LogisticsService.Infrastructure/"]
COPY ["src/Services/Logistics/Ecommerce.LogisticsService.Api/Ecommerce.LogisticsService.Api.csproj", "src/Services/Logistics/Ecommerce.LogisticsService.Api/"]
RUN dotnet restore "src/Services/Logistics/Ecommerce.LogisticsService.Api/Ecommerce.LogisticsService.Api.csproj"

COPY . .
RUN dotnet publish "src/Services/Logistics/Ecommerce.LogisticsService.Api/Ecommerce.LogisticsService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.LogisticsService.Api.dll"]
