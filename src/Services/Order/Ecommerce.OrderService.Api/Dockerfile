FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/SharedKernel/Ecommerce.Contracts/Ecommerce.Contracts.csproj", "src/SharedKernel/Ecommerce.Contracts/"]
COPY ["src/SharedKernel/Ecommerce.Common/Ecommerce.Common.csproj", "src/SharedKernel/Ecommerce.Common/"]
COPY ["src/Orchestration/Ecommerce.ServiceDefaults/Ecommerce.ServiceDefaults.csproj", "src/Orchestration/Ecommerce.ServiceDefaults/"]
COPY ["src/Services/Order/Ecommerce.OrderService.Domain/Ecommerce.OrderService.Domain.csproj", "src/Services/Order/Ecommerce.OrderService.Domain/"]
COPY ["src/Services/Order/Ecommerce.OrderService.Application/Ecommerce.OrderService.Application.csproj", "src/Services/Order/Ecommerce.OrderService.Application/"]
COPY ["src/Services/Order/Ecommerce.OrderService.Infrastructure/Ecommerce.OrderService.Infrastructure.csproj", "src/Services/Order/Ecommerce.OrderService.Infrastructure/"]
COPY ["src/Services/Order/Ecommerce.OrderService.Api/Ecommerce.OrderService.Api.csproj", "src/Services/Order/Ecommerce.OrderService.Api/"]
RUN dotnet restore "src/Services/Order/Ecommerce.OrderService.Api/Ecommerce.OrderService.Api.csproj"

COPY . .
RUN dotnet publish "src/Services/Order/Ecommerce.OrderService.Api/Ecommerce.OrderService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.OrderService.Api.dll"]
