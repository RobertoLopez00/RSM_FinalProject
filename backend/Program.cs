using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// SERVICE REGISTRATION
// ============================================

// Add MVC controllers for API endpoints
builder.Services.AddControllers();

// Add OpenAPI and Swagger documentation
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// ============================================
// DATABASE CONFIGURATION
// ============================================

// Configure Entity Framework Core with SQL Server
// Reads connection string from appsettings.json configuration
var connectionString = builder.Configuration.GetConnectionString("NorthwindDb");
builder.Services.AddDbContext<NorthwindDbContext>(options =>
    options.UseSqlServer(connectionString));

// ============================================
// BUSINESS LOGIC SERVICES
// ============================================

// Register Order service for CRUD operations
builder.Services.AddScoped<IOrderService, OrderService>();

// Register Address validation service with Google Maps API integration
builder.Services.AddScoped<IAddressValidationService, AddressValidationService>();
builder.Services.AddHttpClient<IAddressValidationService, AddressValidationService>();

// ============================================
// CORS POLICY
// ============================================

// Configure Cross-Origin Resource Sharing to allow frontend applications
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVue3Frontend", policy =>
    {
        // Allow multiple development and production URLs
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:9000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// ============================================
// MIDDLEWARE CONFIGURATION
// ============================================

// Enable Swagger/OpenAPI documentation in development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Enable CORS with configured policy
app.UseCors("AllowVue3Frontend");

// Map API controllers to routes
app.MapControllers();

// Start the application
app.Run();
