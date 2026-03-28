FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/SharedKernel/Ecommerce.Contracts/Ecommerce.Contracts.csproj", "src/SharedKernel/Ecommerce.Contracts/"]
COPY ["src/SharedKernel/Ecommerce.Common/Ecommerce.Common.csproj", "src/SharedKernel/Ecommerce.Common/"]
COPY ["src/Orchestration/Ecommerce.ServiceDefaults/Ecommerce.ServiceDefaults.csproj", "src/Orchestration/Ecommerce.ServiceDefaults/"]
COPY ["src/Services/Basket/Ecommerce.Basket.Domain/Ecommerce.Basket.Domain.csproj", "src/Services/Basket/Ecommerce.Basket.Domain/"]
COPY ["src/Services/Basket/Ecommerce.Basket.Application/Ecommerce.Basket.Application.csproj", "src/Services/Basket/Ecommerce.Basket.Application/"]
COPY ["src/Services/Basket/Ecommerce.Basket.Infrastructure/Ecommerce.Basket.Infrastructure.csproj", "src/Services/Basket/Ecommerce.Basket.Infrastructure/"]
COPY ["src/Services/Basket/Ecommerce.Basket.Api/Ecommerce.Basket.Api.csproj", "src/Services/Basket/Ecommerce.Basket.Api/"]
RUN dotnet restore "src/Services/Basket/Ecommerce.Basket.Api/Ecommerce.Basket.Api.csproj"

COPY . .
RUN dotnet publish "src/Services/Basket/Ecommerce.Basket.Api/Ecommerce.Basket.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Basket.Api.dll"]
