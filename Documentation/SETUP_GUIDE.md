# Northwind Order Management System - Setup Guide

## Overview
This document provides complete setup instructions for the Northwind Order Management System, including database configuration, API key setup, and deployment guidelines.

## System Requirements

- **Backend**: .NET 10.0 SDK
- **Frontend**: Node.js 18+ with npm
- **Database**: SQL Server 2019+ or Azure SQL Database
- **Development Tools**: Visual Studio Code, Docker (optional)

## Part 1: Database Configuration

### 1.1 SQL Server Setup

#### Local Development (Windows/Mac/Linux)

```bash
# Install SQL Server (Docker)
docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=YourPassword123!' \
  -p 1433:1433 \
  -d mcr.microsoft.com/mssql/server:2019-latest
```

#### Connection String
```
Server=localhost,1433;Database=Northwind;User Id=sa;Password=YourPassword123!;
```

### 1.2 Database Initialization

```bash
# Navigate to backend directory
cd backend

# Apply migrations to create Northwind schema
dotnet ef database update

# Verify connection
sqlcmd -S localhost -U sa -P YourPassword123! -Q "SELECT @@VERSION"
```

### 1.3 Northwind Schema

The system uses the classic Northwind database schema with the following tables:

- **Orders** - Order header information
- **OrderDetails** - Line items for each order
- **Customers** - Customer master data
- **Employees** - Employee records
- **Shippers** - Shipping company information
- **Products** - Product catalog

Key relationships:
- `Orders` → `Customers` (many-to-one)
- `Orders` → `Employees` (many-to-one)
- `Orders` → `Shippers` (many-to-one)
- `Orders` → `OrderDetails` (one-to-many)
- `OrderDetails` → `Products` (many-to-one)

## Part 2: Google Maps API Setup

### 2.1 Create Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project named "Northwind-Track"
3. Enable the following APIs:
   - **Maps JavaScript API** (for embedded maps)
   - **Address Validation API** (for address standardization)
   - **Geocoding API** (for coordinate conversion)

### 2.2 Create API Key

1. Navigate to **Credentials** → **Create Credentials** → **API Key**
2. Restrict the key:
   - **HTTP referrers (web sites)**: `http://localhost:9000`, `http://localhost:5096`
   - **APIs**: Select only Maps JavaScript API, Address Validation API, Geocoding API

### 2.3 Environment Configuration

#### Backend (`appsettings.Development.json`)
```json
{
  "GoogleMaps": {
    "ApiKey": "YOUR_GOOGLE_MAPS_API_KEY"
  }
}
```

#### Frontend (`.env.development`)
```
VITE_GOOGLE_MAPS_API_KEY=YOUR_GOOGLE_MAPS_API_KEY
VITE_API_BASE_URL=http://localhost:5096
```

### 2.4 API Usage

**Address Validation Endpoint**
```
POST /api/addresses/validate
{
  "address": "1600 Amphitheatre Parkway",
  "city": "Mountain View",
  "state": "CA",
  "postalCode": "94043",
  "country": "USA"
}
```

**Response**
```json
{
  "isValid": true,
  "normalizedAddress": "1600 Amphitheater Parkway, Mountain View, CA 94043, USA",
  "latitude": 37.4224764,
  "longitude": -122.0842499
}
```

## Part 3: Backend API Configuration

### 3.1 Application Setup

```bash
cd backend
dotnet restore
dotnet build
```

### 3.2 Running the Backend

**Development Mode**
```bash
dotnet watch run
# API available at: http://localhost:5096
# Swagger UI: http://localhost:5096/swagger
```

**Production Mode**
```bash
dotnet build -c Release
dotnet bin/Release/net10.0/backend.dll
```

### 3.3 API Endpoints

#### Orders Management

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/orders` | Get all orders |
| GET | `/api/orders/{id}` | Get order by ID |
| POST | `/api/orders` | Create new order |
| PUT | `/api/orders/{id}` | Update order |
| DELETE | `/api/orders/{id}` | Delete order |
| GET | `/api/orders/customer/{customerId}` | Get orders by customer |
| GET | `/api/orders/date-range` | Get orders by date range |

#### Address Validation

| Method | Endpoint | Purpose |
|--------|----------|---------|
| POST | `/api/addresses/validate` | Validate and standardize address |
| GET | `/api/lookups/countries` | Get countries list |
| GET | `/api/lookups/shippers` | Get shippers list |
| GET | `/api/lookups/employees` | Get employees list |

### 3.4 Create Order Example

```bash
curl -X POST http://localhost:5096/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerID": "ALFKI",
    "employeeID": 1,
    "shipVia": 1,
    "shipName": "Alfreds Futterkiste",
    "shipAddress": "Obere Str. 57",
    "shipCity": "Berlin",
    "shipRegion": "Berlin",
    "shipPostalCode": "12209",
    "shipCountry": "Germany",
    "orderDetails": [
      {
        "productID": 1,
        "quantity": 10,
        "unitPrice": 18.00,
        "discount": 0
      }
    ]
  }'
```

## Part 4: Frontend Setup

### 4.1 Installation

```bash
cd frontend
npm install
```

### 4.2 Development Server

```bash
# Start development server
npm run dev

# Frontend available at: http://localhost:9000
```

### 4.3 Build for Production

```bash
npm run build

# Output in: frontend/dist/spa
```

### 4.4 Frontend Environment Variables

Create `.env.development.local`:
```
VITE_API_BASE_URL=http://localhost:5096
VITE_GOOGLE_MAPS_API_KEY=YOUR_API_KEY
```

## Part 5: Testing

### 5.1 Backend Unit Tests

**Run Tests**
```bash
cd backend
dotnet test backend.Tests
```

**Test Coverage**
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=json
```

**Test Files**
- `backend.Tests/OrderServiceTests.cs` - Business logic tests (12 tests)
- `backend.Tests/OrdersControllerTests.cs` - API endpoint tests (16 tests)

**Coverage Goals**
- Orders Service: 95%
- Orders Controller: 90%
- Overall Target: 80%

### 5.2 Frontend Tests (Bonus)

```bash
cd frontend
npm run test
```

## Part 6: Deployment

### 6.1 Docker Deployment

**Build Backend Image**
```bash
cd backend
docker build -t northwind-api:latest .
```

**Build Frontend Image**
```bash
cd frontend
docker build -t northwind-ui:latest .
```

**Docker Compose**
```bash
cd ..
docker-compose up -d
```

### 6.2 Environment Variables for Production

```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=db;Database=Northwind;User Id=sa;Password=YourSecurePassword!;
GoogleMaps__ApiKey=YOUR_PRODUCTION_API_KEY
```

### 6.3 Health Checks

```bash
# Backend health
curl http://localhost:5096/health

# Frontend
curl http://localhost:9000
```

## Part 7: Troubleshooting

### Database Connection Issues

**Error**: `Cannot connect to database`
```
Solution:
1. Verify SQL Server is running: docker ps
2. Check connection string in appsettings.json
3. Ensure firewall allows port 1433
4. Test connection: sqlcmd -S localhost -U sa -P password
```

### Google Maps API Errors

**Error**: `Maps API key not valid`
```
Solution:
1. Verify API key in environment variables
2. Check that APIs are enabled in Google Cloud Console
3. Verify referrer restrictions match your domain
4. Regenerate key if needed
```

### Frontend Build Issues

**Error**: `Module not found`
```
Solution:
cd frontend
rm -rf node_modules
npm install
npm run build
```

### Port Already in Use

```bash
# Find process using port
lsof -i :5096  # Backend
lsof -i :9000  # Frontend

# Kill process (macOS/Linux)
kill -9 <PID>

# Or use different ports
dotnet watch run -- --urls="http://localhost:5097"
```

## Part 8: Maintenance

### Database Backups

```bash
# Backup Northwind database
sqlcmd -S localhost -U sa -P password \
  -Q "BACKUP DATABASE Northwind TO DISK='/var/opt/mssql/backup/northwind.bak'"
```

### Log Files

- **Backend**: `backend/bin/Debug/net10.0/logs/`
- **Frontend**: Browser console (F12)

### Performance Monitoring

```bash
# Check API response times
dotnet trace collect -- dotnet run

# Analyze frontend bundle
npm run build --report
```

## Part 9: Security Considerations

### API Authentication (Future Enhancement)

```csharp
// Add JWT authentication to Program.cs
builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer(options => { /* config */ });
```

### HTTPS Configuration

```json
{
  "Kestrel": {
    "EndpointDefaults": {
      "Protocols": "Http2"
    },
    "Endpoints": {
      "HttpsInlineCertAndKey": {
        "Url": "https://localhost:5001",
        "Certificate": { /* cert config */ }
      }
    }
  }
}
```

### CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", builder =>
    {
        builder.WithOrigins("http://localhost:9000")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
```

## Support

For issues or questions:
1. Check this guide's troubleshooting section
2. Review API documentation at `/swagger`
3. Check browser console for frontend errors
4. Review application logs

---
