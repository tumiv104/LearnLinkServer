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

COPY --from=build /app/publish .

# Render requires port 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "API.dll"]
