FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PaymentsService/*.csproj PaymentsService/
COPY SharedContracts/*.csproj SharedContracts/
RUN dotnet restore PaymentsService/PaymentsService.csproj

COPY . .
RUN dotnet publish PaymentsService/PaymentsService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PaymentsService.dll"]
