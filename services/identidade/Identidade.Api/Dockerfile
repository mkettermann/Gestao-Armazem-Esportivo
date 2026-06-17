FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY shared/ shared/
COPY services/identidade/ services/identidade/

RUN dotnet restore services/identidade/Identidade.Api/Identidade.Api.csproj
RUN dotnet publish services/identidade/Identidade.Api/Identidade.Api.csproj \
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
ENTRYPOINT ["dotnet", "Identidade.Api.dll"]
