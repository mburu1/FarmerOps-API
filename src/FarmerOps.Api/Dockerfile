FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY src/FarmerOps.Domain/FarmerOps.Domain.csproj src/FarmerOps.Domain/
COPY src/FarmerOps.Application/FarmerOps.Application.csproj src/FarmerOps.Application/
COPY src/FarmerOps.Infrastructure/FarmerOps.Infrastructure.csproj src/FarmerOps.Infrastructure/
COPY src/FarmerOps.Api/FarmerOps.Api.csproj src/FarmerOps.Api/
RUN dotnet restore src/FarmerOps.Api/FarmerOps.Api.csproj

COPY src/ src/
RUN dotnet publish src/FarmerOps.Api/FarmerOps.Api.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FarmerOps.Api.dll"]
