# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy the project file
COPY ["backend.csproj", "./"]

# Restore dependencies
RUN dotnet restore "backend.csproj"

# Copy the entire source code
COPY . .

# Build the application
RUN dotnet build "backend.csproj" -c Release -o /app/build

# Publish stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Copy the built application from build stage
COPY --from=build /app/build .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "backend.dll"]
