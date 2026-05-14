# ===============================
# STAGE 1: Build .NET application
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and projects
COPY ["LearnLinkServer.sln", "./"]
COPY ["API/API.csproj", "API/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

# Restore dependencies
RUN dotnet restore "LearnLinkServer.sln"

# Copy all source code
COPY . .

# Build and publish app
WORKDIR "/src/API"
RUN dotnet publish "API.csproj" -c Release -o /app/publish

# ===============================
# STAGE 2: Runtime environment
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy from build stage
COPY --from=build /app/publish .

# Expose port 8080 (Render expects this)
EXPOSE 8080

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "API.dll"]
