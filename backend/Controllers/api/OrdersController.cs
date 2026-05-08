using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.DTOs;

namespace backend.Controllers.api;

/// <summary>
/// Orders API Controller
/// Handles HTTP requests for order management (CRUD operations)
/// All endpoints return JSON responses following REST conventions
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    /// <summary>
    /// Constructor: Dependency injection for order service and logging
    /// </summary>
    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/orders
    /// Retrieves all orders from the database
    /// </summary>
    /// <returns>List of all orders with customer, employee, and shipper details</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrderDto>>> GetAll()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all orders");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error fetching orders");
        }
    }

    /// <summary>
    /// GET /api/orders/{id}
    /// Retrieves a specific order by its ID
    /// </summary>
    /// <param name="id">Order ID to retrieve</param>
    /// <returns>Order details if found, 404 if not found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound($"Order {id} not found");

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching order {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error fetching order");
        }
    }

    /// <summary>
    /// POST /api/orders
    /// Creates a new order with order details (line items)
    /// </summary>
    /// <param name="createOrderDto">Order data including customer, dates, and line items</param>
    /// <returns>Created order with generated OrderID, 201 Created</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto createOrderDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var order = await _orderService.CreateOrderAsync(createOrderDto);
            return CreatedAtAction(nameof(GetById), new { id = order.OrderID }, order);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid data for order creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error creating order");
        }
    }

    /// <summary>
    /// PUT /api/orders/{id}
    /// Updates an existing order's details and line items
    /// </summary>
    /// <param name="id">Order ID to update</param>
    /// <param name="updateOrderDto">Updated order data</param>
    /// <returns>Updated order data, 404 if order not found</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> Update(int id, [FromBody] UpdateOrderDto updateOrderDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var order = await _orderService.UpdateOrderAsync(id, updateOrderDto);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Order not found: {OrderId}", id);
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid data for order update {OrderId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating order");
        }
    }

    /// <summary>
    /// DELETE /api/orders/{id}
    /// Deletes an order and its associated line items
    /// </summary>
    /// <param name="id">Order ID to delete</param>
    /// <returns>204 No Content on success, 404 if order not found</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var success = await _orderService.DeleteOrderAsync(id);
            if (!success)
                return NotFound($"Order {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting order");
        }
    }

    /// <summary>
    /// GET /api/orders/customer/{customerId}
    /// Retrieves all orders for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID to filter orders</param>
    /// <returns>List of orders belonging to the customer</returns>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrderDto>>> GetByCustomer(string customerId)
    {
        try
        {
            var orders = await _orderService.GetOrdersByCustomerAsync(customerId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders for customer {CustomerId}", customerId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error fetching orders");
        }
    }

    /// <summary>
    /// GET /api/orders/date-range
    /// Retrieves orders within a specified date range
    /// </summary>
    /// <param name="startDate">Start date for the range (query parameter)</param>
    /// <param name="endDate">End date for the range (query parameter)</param>
    /// <returns>List of orders within the specified date range</returns>
    [HttpGet("date-range")]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrderDto>>> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var orders = await _orderService.GetOrdersByDateRangeAsync(startDate, endDate);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders for date range");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error fetching orders");
        }
    }
}
