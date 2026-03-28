FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/SharedKernel/Ecommerce.Contracts/Ecommerce.Contracts.csproj", "src/SharedKernel/Ecommerce.Contracts/"]
COPY ["src/SharedKernel/Ecommerce.Common/Ecommerce.Common.csproj", "src/SharedKernel/Ecommerce.Common/"]
COPY ["src/Orchestration/Ecommerce.ServiceDefaults/Ecommerce.ServiceDefaults.csproj", "src/Orchestration/Ecommerce.ServiceDefaults/"]
COPY ["src/Services/Basket/Ecommerce.BasketService.Domain/Ecommerce.BasketService.Domain.csproj", "src/Services/Basket/Ecommerce.BasketService.Domain/"]
COPY ["src/Services/Basket/Ecommerce.BasketService.Application/Ecommerce.BasketService.Application.csproj", "src/Services/Basket/Ecommerce.BasketService.Application/"]
COPY ["src/Services/Basket/Ecommerce.BasketService.Infrastructure/Ecommerce.BasketService.Infrastructure.csproj", "src/Services/Basket/Ecommerce.BasketService.Infrastructure/"]
COPY ["src/Services/Basket/Ecommerce.BasketService.Api/Ecommerce.BasketService.Api.csproj", "src/Services/Basket/Ecommerce.BasketService.Api/"]
RUN dotnet restore "src/Services/Basket/Ecommerce.BasketService.Api/Ecommerce.BasketService.Api.csproj"

COPY . .
RUN dotnet publish "src/Services/Basket/Ecommerce.BasketService.Api/Ecommerce.BasketService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.BasketService.Api.dll"]

