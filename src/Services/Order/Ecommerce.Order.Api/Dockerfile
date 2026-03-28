FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/SharedKernel/Ecommerce.Contracts/Ecommerce.Contracts.csproj", "src/SharedKernel/Ecommerce.Contracts/"]
COPY ["src/SharedKernel/Ecommerce.Common/Ecommerce.Common.csproj", "src/SharedKernel/Ecommerce.Common/"]
COPY ["src/Orchestration/Ecommerce.ServiceDefaults/Ecommerce.ServiceDefaults.csproj", "src/Orchestration/Ecommerce.ServiceDefaults/"]
COPY ["src/Services/Order/Ecommerce.Order.Domain/Ecommerce.Order.Domain.csproj", "src/Services/Order/Ecommerce.Order.Domain/"]
COPY ["src/Services/Order/Ecommerce.Order.Application/Ecommerce.Order.Application.csproj", "src/Services/Order/Ecommerce.Order.Application/"]
COPY ["src/Services/Order/Ecommerce.Order.Infrastructure/Ecommerce.Order.Infrastructure.csproj", "src/Services/Order/Ecommerce.Order.Infrastructure/"]
COPY ["src/Services/Order/Ecommerce.Order.Api/Ecommerce.Order.Api.csproj", "src/Services/Order/Ecommerce.Order.Api/"]
RUN dotnet restore "src/Services/Order/Ecommerce.Order.Api/Ecommerce.Order.Api.csproj"

COPY . .
RUN dotnet publish "src/Services/Order/Ecommerce.Order.Api/Ecommerce.Order.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Order.Api.dll"]
