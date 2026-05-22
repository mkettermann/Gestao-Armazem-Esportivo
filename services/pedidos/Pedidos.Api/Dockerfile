FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY shared/ shared/
COPY services/pedidos/ services/pedidos/

RUN dotnet restore services/pedidos/Pedidos.Api/Pedidos.Api.csproj
RUN dotnet publish services/pedidos/Pedidos.Api/Pedidos.Api.csproj \
	-c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
COPY --from=build /app/publish .
USER appuser
EXPOSE 8080
ENTRYPOINT ["dotnet", "Pedidos.Api.dll"]
