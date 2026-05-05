FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY GreenhouseGuardian.sln .
COPY src/GreenhouseGuardian.API/GreenhouseGuardian.API.csproj src/GreenhouseGuardian.API/
COPY src/GreenhouseGuardian.Application/GreenhouseGuardian.Application.csproj src/GreenhouseGuardian.Application/
COPY src/GreenhouseGuardian.Domain/GreenhouseGuardian.Domain.csproj src/GreenhouseGuardian.Domain/
COPY src/GreenhouseGuardian.Infrastructure/GreenhouseGuardian.Infrastructure.csproj src/GreenhouseGuardian.Infrastructure/
COPY src/GreenhouseGuardian.Emulator/GreenhouseGuardian.Emulator.csproj src/GreenhouseGuardian.Emulator/

RUN dotnet restore

COPY . .
WORKDIR /src/src/GreenhouseGuardian.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GreenhouseGuardian.API.dll"]
