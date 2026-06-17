FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY shared/ shared/
COPY services/catalogo/ services/catalogo/

RUN dotnet restore services/catalogo/Catalogo.Api/Catalogo.Api.csproj
RUN dotnet publish services/catalogo/Catalogo.Api/Catalogo.Api.csproj \
	-c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
# curl é usado pelo healthcheck do docker-compose para sondar o endpoint /health.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
COPY --from=build /app/publish .
USER appuser
EXPOSE 8080
ENTRYPOINT ["dotnet", "Catalogo.Api.dll"]
