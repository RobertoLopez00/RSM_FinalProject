# Docker Setup Guide

This project uses Docker and Docker Compose to run all services (frontend, backend, and database) in containers.

## Prerequisites

- Docker Desktop installed and running
- Docker Compose (included with Docker Desktop)

## Services

- **Database**: SQL Server 2022 (port 1433)
- **Backend**: ASP.NET Core 10 API (port 8080)
- **Frontend**: Quasar/Vue.js (port 80)

## Quick Start

### Build and run all services:
```bash
docker-compose up --build
```

### Run without rebuilding (if images already exist):
```bash
docker-compose up
```

### Stop all services:
```bash
docker-compose down
```

### Stop and remove all volumes (clean slate):
```bash
docker-compose down -v
```

## Accessing Services

- **Frontend**: http://localhost
- **Backend API**: http://localhost:8080
- **Database**: localhost:1433 (SQL Server)
  - Username: `sa`
  - Password: `RsmAcademic123!`

## Development Workflow

### View logs from all services:
```bash
docker-compose logs -f
```

### View logs from a specific service:
```bash
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f database
```

### Rebuild a specific service:
```bash
docker-compose up --build backend
```

### Execute commands in a running container:
```bash
# Backend
docker-compose exec backend dotnet --version

# Frontend
docker-compose exec frontend npm list

# Database
docker-compose exec database /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P RsmAcademic123!
```

## Environment Variables

Database credentials are set in `docker-compose.yml`:
- **SA_PASSWORD**: `RsmAcademic123!`
- **Database**: `Northwind`

Backend will automatically connect to the database container using the connection string defined in the compose file.

## Troubleshooting

### Database won't start
- Check if port 1433 is already in use
- Ensure Docker has enough resources allocated

### Frontend can't connect to backend
- Verify backend is healthy: `docker-compose logs backend`
- Check the nginx configuration in `frontend/nginx.conf`

### Clear everything and start fresh
```bash
docker-compose down -v
docker-compose up --build
```

## Volume Persistence

SQL Server data is stored in a Docker volume (`sqlserver_data`). This persists data between restarts but will be deleted with `docker-compose down -v`.

## Production Notes

For production deployment:
1. Use environment files (`.env`) instead of hardcoding credentials
2. Change default database password
3. Use proper SSL certificates
4. Consider using a secrets manager
5. Set proper resource limits in the compose file
