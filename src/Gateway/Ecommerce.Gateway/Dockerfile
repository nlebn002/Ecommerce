FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/Orchestration/Ecommerce.ServiceDefaults/Ecommerce.ServiceDefaults.csproj", "src/Orchestration/Ecommerce.ServiceDefaults/"]
COPY ["src/Gateway/Ecommerce.Gateway/Ecommerce.Gateway.csproj", "src/Gateway/Ecommerce.Gateway/"]
RUN dotnet restore "src/Gateway/Ecommerce.Gateway/Ecommerce.Gateway.csproj"

COPY . .
RUN dotnet publish "src/Gateway/Ecommerce.Gateway/Ecommerce.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Gateway.dll"]
