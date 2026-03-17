FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SimitConsulta.Domain/SimitConsulta.Domain.csproj \
     SimitConsulta.Domain/
COPY SimitConsulta.Application/SimitConsulta.Application.csproj \
     SimitConsulta.Application/
COPY SimitConsulta.Infrastructure/SimitConsulta.Infrastructure.csproj \
     SimitConsulta.Infrastructure/
COPY SimitConsulta.API/SimitConsulta.API.csproj \
     SimitConsulta.API/

RUN dotnet restore SimitConsulta.API/SimitConsulta.API.csproj

COPY SimitConsulta.Domain/        SimitConsulta.Domain/
COPY SimitConsulta.Application/   SimitConsulta.Application/
COPY SimitConsulta.Infrastructure/ SimitConsulta.Infrastructure/
COPY SimitConsulta.API/           SimitConsulta.API/

RUN dotnet publish SimitConsulta.API/SimitConsulta.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SimitConsulta.API.dll"]