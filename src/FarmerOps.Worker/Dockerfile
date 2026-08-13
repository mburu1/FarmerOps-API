FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY src/FarmerOps.Domain/FarmerOps.Domain.csproj src/FarmerOps.Domain/
COPY src/FarmerOps.Application/FarmerOps.Application.csproj src/FarmerOps.Application/
COPY src/FarmerOps.Infrastructure/FarmerOps.Infrastructure.csproj src/FarmerOps.Infrastructure/
COPY src/FarmerOps.Worker/FarmerOps.Worker.csproj src/FarmerOps.Worker/
RUN dotnet restore src/FarmerOps.Worker/FarmerOps.Worker.csproj

COPY src/ src/
RUN dotnet publish src/FarmerOps.Worker/FarmerOps.Worker.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FarmerOps.Worker.dll"]
