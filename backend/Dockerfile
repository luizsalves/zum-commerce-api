# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props ZumCommerce.Backend.slnx ./
COPY src/ src/
RUN dotnet restore src/Core.Web/Core.Web.csproj
RUN dotnet publish src/Core.Web/Core.Web.csproj -c Release -o /app --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "Core.Web.dll"]
