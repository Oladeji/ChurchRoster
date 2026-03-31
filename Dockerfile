# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY backend/ChurchRoster.Api/ChurchRoster.Api.csproj backend/ChurchRoster.Api/
COPY backend/ChurchRoster.Application/ChurchRoster.Application.csproj backend/ChurchRoster.Application/
COPY backend/ChurchRoster.Core/ChurchRoster.Core.csproj backend/ChurchRoster.Core/
COPY backend/ChurchRoster.Infrastructure/ChurchRoster.Infrastructure.csproj backend/ChurchRoster.Infrastructure/
COPY backend/Directory.Packages.props backend/

# Restore dependencies
RUN dotnet restore backend/ChurchRoster.Api/ChurchRoster.Api.csproj

# Copy all source code
COPY backend/ backend/

# Build and publish the application
WORKDIR /src/backend/ChurchRoster.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published application from build stage
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "ChurchRoster.Api.dll"]
