using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Entities;

namespace backend.Services;

/// <summary>
/// Order Service Interface
/// Defines contract for order-related business operations
/// </summary>
public interface IOrderService
{
    /// <summary>Retrieves all orders from database</summary>
    Task<List<OrderDto>> GetAllOrdersAsync();

    /// <summary>Retrieves a specific order by ID</summary>
    Task<OrderDto?> GetOrderByIdAsync(int orderId);

    /// <summary>Creates a new order with line items</summary>
    Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);

    /// <summary>Updates an existing order and its line items</summary>
    Task<OrderDto> UpdateOrderAsync(int orderId, UpdateOrderDto updateOrderDto);

    /// <summary>Deletes an order and its associated line items</summary>
    Task<bool> DeleteOrderAsync(int orderId);

    /// <summary>Retrieves all orders for a specific customer</summary>
    Task<List<OrderDto>> GetOrdersByCustomerAsync(string customerId);

    /// <summary>Retrieves orders within a date range</summary>
    Task<List<OrderDto>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
}

/// <summary>
/// Order Service Implementation
/// Handles business logic for order CRUD operations, validation, and data mapping
/// Uses Entity Framework Core for database operations
/// </summary>
public class OrderService : IOrderService
{
    private readonly NorthwindDbContext _context;

    /// <summary>
    /// Constructor: Initializes service with database context
    /// </summary>
    public OrderService(NorthwindDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all orders with related data (customer, employee, shipper, order details)
    /// </summary>
    public async Task<List<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.Shipper)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .ToListAsync();

        return MapOrdersToDto(orders);
    }

    /// <summary>
    /// Retrieves a single order by ID, including all related entities
    /// </summary>
    public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.Shipper)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.OrderID == orderId);

        if (order == null) return null;

        return MapOrderToDto(order);
    }

    /// <summary>
    /// Creates a new order with validation of shipping fields
    /// Validates that all referenced products exist
    /// </summary>
    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
    {
        // Validate shipping field lengths before creating order
        ValidateShippingFieldLengths(
            createOrderDto.ShipName,
            createOrderDto.ShipAddress,
            createOrderDto.ShipCity,
            createOrderDto.ShipRegion,
            createOrderDto.ShipPostalCode,
            createOrderDto.ShipCountry
        );

        // Create new Order entity
        var order = new Order
        {
            CustomerID = createOrderDto.CustomerID,
            EmployeeID = createOrderDto.EmployeeID,
            OrderDate = createOrderDto.OrderDate ?? DateTime.Now,
            RequiredDate = createOrderDto.RequiredDate,
            ShippedDate = createOrderDto.ShippedDate,
            ShipVia = createOrderDto.ShipVia,
            Freight = createOrderDto.Freight,
            ShipName = createOrderDto.ShipName,
            ShipAddress = createOrderDto.ShipAddress,
            ShipCity = createOrderDto.ShipCity,
            ShipRegion = createOrderDto.ShipRegion,
            ShipPostalCode = createOrderDto.ShipPostalCode,
            ShipCountry = createOrderDto.ShipCountry
        };

        // Add order details (line items) to the order
        foreach (var detailDto in createOrderDto.OrderDetails)
        {
            // Verify product exists before adding
            var product = await _context.Products.FindAsync(detailDto.ProductID);
            if (product == null)
                throw new ArgumentException($"Product {detailDto.ProductID} not found");

            var orderDetail = new OrderDetail
            {
                ProductID = detailDto.ProductID,
                UnitPrice = detailDto.UnitPrice,
                Quantity = detailDto.Quantity,
                Discount = detailDto.Discount
            };

            order.OrderDetails.Add(orderDetail);
        }

        // Save order to database
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Return the created order with all details
        return await GetOrderByIdAsync(order.OrderID) ?? throw new InvalidOperationException("Order creation failed");
    }

    /// <summary>
    /// Updates an existing order with validation
    /// Supports partial updates using nullable properties (null = no change)
    /// Can update line items (replaces all existing order details)
    /// </summary>
    public async Task<OrderDto> UpdateOrderAsync(int orderId, UpdateOrderDto updateOrderDto)
    {
        // Fetch order with its order details
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.OrderID == orderId);
        
        if (order == null)
            throw new KeyNotFoundException($"Order {orderId} not found");

        // Validate shipping field lengths
        ValidateShippingFieldLengths(
            updateOrderDto.ShipName,
            updateOrderDto.ShipAddress,
            updateOrderDto.ShipCity,
            updateOrderDto.ShipRegion,
            updateOrderDto.ShipPostalCode,
            updateOrderDto.ShipCountry
        );

        // Update properties only if new value is provided. Date fields can also
        // be explicitly cleared by the edit form.
        order.CustomerID = updateOrderDto.CustomerID ?? order.CustomerID;
        order.EmployeeID = updateOrderDto.EmployeeID ?? order.EmployeeID;
        order.OrderDate = updateOrderDto.OrderDate ?? order.OrderDate;
        order.RequiredDate = updateOrderDto.ClearRequiredDate ? null : updateOrderDto.RequiredDate ?? order.RequiredDate;
        order.ShippedDate = updateOrderDto.ClearShippedDate ? null : updateOrderDto.ShippedDate ?? order.ShippedDate;
        order.ShipVia = updateOrderDto.ShipVia ?? order.ShipVia;
        order.Freight = updateOrderDto.Freight ?? order.Freight;
        order.ShipName = updateOrderDto.ShipName ?? order.ShipName;
        order.ShipAddress = updateOrderDto.ShipAddress ?? order.ShipAddress;
        order.ShipCity = updateOrderDto.ShipCity ?? order.ShipCity;
        order.ShipRegion = updateOrderDto.ShipRegion ?? order.ShipRegion;
        order.ShipPostalCode = updateOrderDto.ShipPostalCode ?? order.ShipPostalCode;
        order.ShipCountry = updateOrderDto.ShipCountry ?? order.ShipCountry;

        // Update order details if provided (replaces entire collection)
        if (updateOrderDto.OrderDetails is not null)
        {
            if (updateOrderDto.OrderDetails.Count == 0)
                throw new ArgumentException("Order must include at least one detail line");

            // Remove all existing order details
            _context.OrderDetails.RemoveRange(order.OrderDetails);

            // Add new order details
            foreach (var detailDto in updateOrderDto.OrderDetails)
            {
                // Verify product exists
                var product = await _context.Products.FindAsync(detailDto.ProductID);
                if (product == null)
                    throw new ArgumentException($"Product {detailDto.ProductID} not found");

                order.OrderDetails.Add(new OrderDetail
                {
                    OrderID = orderId,
                    ProductID = detailDto.ProductID,
                    UnitPrice = detailDto.UnitPrice,
                    Quantity = detailDto.Quantity,
                    Discount = detailDto.Discount
                });
            }
        }

        // Save changes to database
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

        // Return updated order with all details
        return await GetOrderByIdAsync(orderId) ?? throw new InvalidOperationException("Order update failed");
    }

    /// <summary>
    /// Deletes an order and its associated order details
    /// Uses ExecuteDeleteAsync for efficiency (single DELETE statement)
    /// Falls back to traditional method if batch delete fails
    /// </summary>
    public async Task<bool> DeleteOrderAsync(int orderId)
    {
        try
        {
            // Delete order details first (due to foreign key constraint)
            await _context.OrderDetails
                .Where(od => od.OrderID == orderId)
                .ExecuteDeleteAsync();

            // Delete the order
            var deleted = await _context.Orders
                .Where(o => o.OrderID == orderId)
                .ExecuteDeleteAsync();

            return deleted > 0;
        }
        catch
        {
            // Fallback: try traditional deletion method if batch delete fails
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderID == orderId);
            
            if (order == null)
                return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    /// <summary>
    /// Retrieves all orders for a specific customer
    /// </summary>
    public async Task<List<OrderDto>> GetOrdersByCustomerAsync(string customerId)
    {
        var orders = await _context.Orders
            .Where(o => o.CustomerID == customerId)
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.Shipper)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .ToListAsync();

        return MapOrdersToDto(orders);
    }

    /// <summary>
    /// Retrieves orders within a specific date range
    /// Orders are filtered by OrderDate
    /// </summary>
    public async Task<List<OrderDto>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var orders = await _context.Orders
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.Shipper)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .ToListAsync();

        return MapOrdersToDto(orders);
    }

    /// <summary>
    /// Maps a list of Order entities to OrderDto (Data Transfer Objects)
    /// </summary>
    private List<OrderDto> MapOrdersToDto(List<Order> orders)
    {
        return orders.Select(o => MapOrderToDto(o)).ToList();
    }

    /// <summary>
    /// Maps a single Order entity to OrderDto
    /// Calculates line totals and total order amount
    /// Combines employee first and last name
    /// </summary>
    private OrderDto MapOrderToDto(Order order)
    {
        var orderDto = new OrderDto
        {
            OrderID = order.OrderID,
            CustomerID = order.CustomerID,
            CustomerName = order.Customer?.CompanyName,
            EmployeeID = order.EmployeeID,
            EmployeeName = order.Employee != null ? $"{order.Employee.FirstName} {order.Employee.LastName}" : null,
            OrderDate = order.OrderDate,
            RequiredDate = order.RequiredDate,
            ShippedDate = order.ShippedDate,
            ShipVia = order.ShipVia,
            ShipperName = order.Shipper?.CompanyName,
            Freight = order.Freight,
            ShipName = order.ShipName,
            ShipAddress = order.ShipAddress,
            ShipCity = order.ShipCity,
            ShipRegion = order.ShipRegion,
            ShipPostalCode = order.ShipPostalCode,
            ShipCountry = order.ShipCountry,
            Status = GetOrderStatus(order),
            // Map order details with calculated line total (UnitPrice * Quantity * (1 - Discount))
            OrderDetails = order.OrderDetails.Select(od => new OrderDetailDto
            {
                OrderID = od.OrderID,
                ProductID = od.ProductID,
                ProductName = od.Product?.ProductName,
                UnitPrice = od.UnitPrice,
                Quantity = od.Quantity,
                Discount = od.Discount,
                LineTotal = od.UnitPrice * od.Quantity * (1 - (decimal)od.Discount)
            }).ToList()
        };

        // Calculate total order amount from all line items
        orderDto.TotalAmount = orderDto.OrderDetails.Sum(od => od.LineTotal);
        return orderDto;
    }

private static string GetOrderStatus(Order order)
{
    var today = DateTime.UtcNow.Date;

    if (order.ShippedDate.HasValue)
        return "Shipped";

    if (order.RequiredDate?.Date < today)
        return "Delayed";

    return "Pending";
}

    /// <summary>
    /// Validates that shipping field lengths do not exceed database column limits
    /// Throws ArgumentException if any field exceeds its max length
    /// </summary>
    private static void ValidateShippingFieldLengths(
        string? shipName,
        string? shipAddress,
        string? shipCity,
        string? shipRegion,
        string? shipPostalCode,
        string? shipCountry)
    {
        ValidateMaxLength(shipName, 40, nameof(shipName));
        ValidateMaxLength(shipAddress, 60, nameof(shipAddress));
        ValidateMaxLength(shipCity, 15, nameof(shipCity));
        ValidateMaxLength(shipRegion, 15, nameof(shipRegion));
        ValidateMaxLength(shipPostalCode, 10, nameof(shipPostalCode));
        ValidateMaxLength(shipCountry, 15, nameof(shipCountry));
    }

    /// <summary>
    /// Validates that a string value does not exceed max length
    /// </summary>
    private static void ValidateMaxLength(string? value, int maxLength, string fieldName)
    {
        if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
            throw new ArgumentException($"{fieldName} exceeds max length of {maxLength} characters");
    }
}
