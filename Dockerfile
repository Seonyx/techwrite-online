# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY src/TechWrite.Web/TechWrite.Web.csproj ./TechWrite.Web/
RUN dotnet restore TechWrite.Web/TechWrite.Web.csproj

# Copy source code and build
COPY src/TechWrite.Web/ ./TechWrite.Web/
WORKDIR /src/TechWrite.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser

# Copy published app
COPY --from=build /app/publish .

# Create SQLite data directory and set ownership
RUN mkdir -p /app/data && chown appuser:appuser /app/data

# Set ownership and switch to non-root user
RUN chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "TechWrite.Web.dll"]
