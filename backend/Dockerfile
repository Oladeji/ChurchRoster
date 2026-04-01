# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ChurchRoster.Api/ChurchRoster.Api.csproj ChurchRoster.Api/
COPY ChurchRoster.Application/ChurchRoster.Application.csproj ChurchRoster.Application/
COPY ChurchRoster.Core/ChurchRoster.Core.csproj ChurchRoster.Core/
COPY ChurchRoster.Infrastructure/ChurchRoster.Infrastructure.csproj ChurchRoster.Infrastructure/
COPY Directory.Packages.props .

# Restore dependencies
RUN dotnet restore ChurchRoster.Api/ChurchRoster.Api.csproj

# Copy all source code
COPY . .

# Build and publish the application
WORKDIR /src/ChurchRoster.Api
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
