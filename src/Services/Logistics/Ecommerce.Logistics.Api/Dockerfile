FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/SharedKernel/Ecommerce.Contracts/Ecommerce.Contracts.csproj", "src/SharedKernel/Ecommerce.Contracts/"]
COPY ["src/SharedKernel/Ecommerce.Common/Ecommerce.Common.csproj", "src/SharedKernel/Ecommerce.Common/"]
COPY ["src/Orchestration/Ecommerce.ServiceDefaults/Ecommerce.ServiceDefaults.csproj", "src/Orchestration/Ecommerce.ServiceDefaults/"]
COPY ["src/Services/Logistics/Ecommerce.Logistics.Domain/Ecommerce.Logistics.Domain.csproj", "src/Services/Logistics/Ecommerce.Logistics.Domain/"]
COPY ["src/Services/Logistics/Ecommerce.Logistics.Application/Ecommerce.Logistics.Application.csproj", "src/Services/Logistics/Ecommerce.Logistics.Application/"]
COPY ["src/Services/Logistics/Ecommerce.Logistics.Infrastructure/Ecommerce.Logistics.Infrastructure.csproj", "src/Services/Logistics/Ecommerce.Logistics.Infrastructure/"]
COPY ["src/Services/Logistics/Ecommerce.Logistics.Api/Ecommerce.Logistics.Api.csproj", "src/Services/Logistics/Ecommerce.Logistics.Api/"]
RUN dotnet restore "src/Services/Logistics/Ecommerce.Logistics.Api/Ecommerce.Logistics.Api.csproj"

COPY . .
RUN dotnet publish "src/Services/Logistics/Ecommerce.Logistics.Api/Ecommerce.Logistics.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Logistics.Api.dll"]
