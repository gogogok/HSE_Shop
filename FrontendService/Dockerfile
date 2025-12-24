FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FrontendService/FrontendService.csproj FrontendService/
RUN dotnet restore FrontendService/FrontendService.csproj

COPY . .
RUN dotnet publish FrontendService/FrontendService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "FrontendService.dll"]
