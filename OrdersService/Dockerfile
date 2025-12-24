FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY OrdersService/*.csproj OrdersService/
COPY SharedContracts/*.csproj SharedContracts/
RUN dotnet restore OrdersService/OrdersService.csproj

COPY . .
RUN dotnet publish OrdersService/OrdersService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "OrdersService.dll"]
