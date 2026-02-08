# 1ª Fase: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY Core/*.csproj ./Core/
COPY Application/*.csproj ./Application/
COPY Infrastructure/*.csproj ./Infrastructure/
COPY AgroSolutions.Sensor.Ingestion/*.csproj ./AgroSolutions.Sensor.Ingestion/

RUN dotnet restore ./AgroSolutions.Sensor.Ingestion/AgroSolutions.Sensor.Ingestion.csproj

COPY . .

RUN dotnet publish ./AgroSolutions.Sensor.Ingestion/AgroSolutions.Sensor.Ingestion.csproj -c Release -o /app --no-restore

# 2ª Fase: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app

RUN addgroup -g 1000 appgroup && adduser -u 1000 -G appgroup -s /bin/sh -D appuser
RUN chown -R appuser:appgroup /app
USER appuser

COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AgroSolutions.Sensor.Ingestion.dll"]
