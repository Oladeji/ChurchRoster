# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["ChurchRoster.Api/ChurchRoster.Api.csproj", "ChurchRoster.Api/"]
COPY ["ChurchRoster.Application/ChurchRoster.Application.csproj", "ChurchRoster.Application/"]
COPY ["ChurchRoster.Core/ChurchRoster.Core.csproj", "ChurchRoster.Core/"]
COPY ["ChurchRoster.Infrastructure/ChurchRoster.Infrastructure.csproj", "ChurchRoster.Infrastructure/"]
COPY ["Directory.Packages.props", "./"]

# Restore dependencies
RUN dotnet restore "ChurchRoster.Api/ChurchRoster.Api.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/ChurchRoster.Api"
RUN dotnet build "ChurchRoster.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "ChurchRoster.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChurchRoster.Api.dll"]
