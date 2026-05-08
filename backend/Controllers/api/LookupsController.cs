using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;

namespace backend.Controllers.api;

[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly NorthwindDbContext _context;
    private readonly ILogger<LookupsController> _logger;

    public LookupsController(NorthwindDbContext context, ILogger<LookupsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers for dropdown
    /// </summary>
    [HttpGet("customers")]
    [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CustomerDto>>> GetCustomers()
    {
        try
        {
            var customers = await _context.Customers
                .OrderBy(c => c.CompanyName)
                .Select(c => new CustomerDto
                {
                    CustomerID = c.CustomerID,
                    CompanyName = c.CompanyName,
                    ContactName = c.ContactName,
                    City = c.City,
                    Country = c.Country,
                    Phone = c.Phone
                })
                .ToListAsync();

            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching customers");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error fetching customers");
        }
    }

    /// <summary>
    /// Get all employees for dropdown
    /// </summary>
    [HttpGet("employees")]
    [ProducesResponseType(typeof(List<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EmployeeDto>>> GetEmployees()
    {
        try
        {
            var employees = await _context.Employees
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Select(e => new EmployeeDto
                {
                    EmployeeID = e.EmployeeID,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Title = e.Title
                })
                .ToListAsync();

            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching employees");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error fetching employees");
        }
    }

    /// <summary>
    /// Get all shippers for dropdown
    /// </summary>
    [HttpGet("shippers")]
    [ProducesResponseType(typeof(List<ShipperDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ShipperDto>>> GetShippers()
    {
        try
        {
            var shippers = await _context.Shippers
                .OrderBy(s => s.CompanyName)
                .Select(s => new ShipperDto
                {
                    ShipperID = s.ShipperID,
                    CompanyName = s.CompanyName,
                    Phone = s.Phone
                })
                .ToListAsync();

            return Ok(shippers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching shippers");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error fetching shippers");
        }
    }

    /// <summary>
    /// Get all products for dropdown
    /// </summary>
    [HttpGet("products")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductDto>>> GetProducts()
    {
        try
        {
            var products = await _context.Products
                .Where(p => !p.Discontinued)
                .OrderBy(p => p.ProductName)
                .Select(p => new ProductDto
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    UnitPrice = p.UnitPrice,
                    UnitsInStock = p.UnitsInStock
                })
                .ToListAsync();

            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error fetching products");
        }
    }
}
