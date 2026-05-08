using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using backend.Controllers.api;
using backend.Services;
using backend.DTOs;
using Microsoft.Extensions.Logging;

namespace backend.Tests;

public class OrdersControllerTests
{
    private Mock<IOrderService> CreateMockOrderService() => new Mock<IOrderService>();
    private Mock<ILogger<OrdersController>> CreateMockLogger() => new Mock<ILogger<OrdersController>>();

    private static TExpected GetInnerResult<TValue, TExpected>(ActionResult<TValue> actionResult)
        where TExpected : class, IActionResult
    {
        return Assert.IsType<TExpected>(actionResult.Result);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithOrders()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        var testOrders = new List<OrderDto> { new OrderDto { OrderID = 1, CustomerID = "CUST1", EmployeeID = 1, Freight = 50 } };

        mockService.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(testOrders);
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.GetAll();

        var okResult = GetInnerResult<List<OrderDto>, OkObjectResult>(result);
        var returnedOrders = Assert.IsType<List<OrderDto>>(okResult.Value);
        Assert.Single(returnedOrders);
    }

    [Fact]
    public async Task GetAll_OnException_Returns500()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        mockService.Setup(s => s.GetAllOrdersAsync()).ThrowsAsync(new Exception("Database error"));
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.GetAll();

        var objectResult = GetInnerResult<List<OrderDto>, ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithOrder()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        var testOrder = new OrderDto { OrderID = 1, CustomerID = "CUST1", EmployeeID = 1, Freight = 50 };

        mockService.Setup(s => s.GetOrderByIdAsync(1)).ReturnsAsync(testOrder);
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.GetById(1);

        var okResult = GetInnerResult<OrderDto, OkObjectResult>(result);
        var returnedOrder = Assert.IsType<OrderDto>(okResult.Value);
        Assert.Equal(1, returnedOrder.OrderID);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        mockService.Setup(s => s.GetOrderByIdAsync(It.IsAny<int>())).ReturnsAsync((OrderDto)null!);
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.GetById(999);

        var notFoundResult = GetInnerResult<OrderDto, NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetById_OnException_Returns500()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        mockService.Setup(s => s.GetOrderByIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Database error"));
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.GetById(1);

        var objectResult = GetInnerResult<OrderDto, ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAtAction()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        var createDto = new CreateOrderDto { CustomerID = "CUST1", EmployeeID = 1, OrderDate = DateTime.Now, ShipVia = 1, Freight = 50, ShipName = "Test", ShipAddress = "123 Main", ShipCity = "NY", ShipCountry = "USA" };
        var createdOrder = new OrderDto { OrderID = 1, CustomerID = "CUST1", EmployeeID = 1, Freight = 50 };

        mockService.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderDto>())).ReturnsAsync(createdOrder);
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.Create(createDto);

        var createdResult = GetInnerResult<OrderDto, CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetById), createdResult.ActionName);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task Create_WithArgumentException_ReturnsBadRequest()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        var createDto = new CreateOrderDto();

        mockService.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderDto>())).ThrowsAsync(new ArgumentException("Invalid data"));
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.Create(createDto);

        var badRequestResult = GetInnerResult<OrderDto, BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Create_OnException_Returns500()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        var createDto = new CreateOrderDto();

        mockService.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderDto>())).ThrowsAsync(new Exception("Database error"));
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.Create(createDto);

        var objectResult = GetInnerResult<OrderDto, ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsOk()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        var updateDto = new UpdateOrderDto { ShipName = "Updated" };
        var updatedOrder = new OrderDto { OrderID = 1, CustomerID = "CUST1", EmployeeID = 1, Freight = 50, ShipName = "Updated" };

        mockService.Setup(s => s.UpdateOrderAsync(It.IsAny<int>(), It.IsAny<UpdateOrderDto>())).ReturnsAsync(updatedOrder);
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.Update(1, updateDto);

        var okResult = GetInnerResult<OrderDto, OkObjectResult>(result);
        var returnedOrder = Assert.IsType<OrderDto>(okResult.Value);
        Assert.Equal("Updated", returnedOrder.ShipName);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        var updateDto = new UpdateOrderDto();

        mockService.Setup(s => s.UpdateOrderAsync(It.IsAny<int>(), It.IsAny<UpdateOrderDto>())).ThrowsAsync(new KeyNotFoundException("Order not found"));
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.Update(999, updateDto);

        var notFoundResult = GetInnerResult<OrderDto, NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task Update_WithArgumentException_ReturnsBadRequest()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        var updateDto = new UpdateOrderDto();

        mockService.Setup(s => s.UpdateOrderAsync(It.IsAny<int>(), It.IsAny<UpdateOrderDto>())).ThrowsAsync(new ArgumentException("Invalid data"));
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.Update(1, updateDto);

        var badRequestResult = GetInnerResult<OrderDto, BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        mockService.Setup(s => s.DeleteOrderAsync(1)).ReturnsAsync(true);
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ReturnsNotFound()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        mockService.Setup(s => s.DeleteOrderAsync(It.IsAny<int>())).ReturnsAsync(false);
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.Delete(999);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task Delete_OnException_Returns500()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        mockService.Setup(s => s.DeleteOrderAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Database error"));
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.Delete(1);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetByCustomer_WithValidCustomerId_ReturnsOk()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        var testOrders = new List<OrderDto> { new OrderDto { OrderID = 1, CustomerID = "CUST1", EmployeeID = 1, Freight = 50 } };

        mockService.Setup(s => s.GetOrdersByCustomerAsync("CUST1")).ReturnsAsync(testOrders);
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.GetByCustomer("CUST1");

        var okResult = GetInnerResult<List<OrderDto>, OkObjectResult>(result);
        var returnedOrders = Assert.IsType<List<OrderDto>>(okResult.Value);
        Assert.Single(returnedOrders);
    }

    [Fact]
    public async Task GetByDateRange_WithValidRange_ReturnsOk()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        var testOrders = new List<OrderDto> { new OrderDto { OrderID = 1, CustomerID = "CUST1", EmployeeID = 1, Freight = 50, OrderDate = DateTime.Now } };
        var startDate = DateTime.Now.AddDays(-7);
        var endDate = DateTime.Now;

        mockService.Setup(s => s.GetOrdersByDateRangeAsync(startDate, endDate)).ReturnsAsync(testOrders);
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.GetByDateRange(startDate, endDate);

        var okResult = GetInnerResult<List<OrderDto>, OkObjectResult>(result);
        var returnedOrders = Assert.IsType<List<OrderDto>>(okResult.Value);
        Assert.Single(returnedOrders);
    }

    [Fact]
    public async Task GetByDateRange_OnException_Returns500()
    {
        var mockService = CreateMockOrderService();
        var mockLogger = CreateMockLogger();
        var startDate = DateTime.Now.AddDays(-7);
        var endDate = DateTime.Now;

        mockService.Setup(s => s.GetOrdersByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ThrowsAsync(new Exception("Database error"));
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        var result = await controller.GetByDateRange(startDate, endDate);

        var objectResult = GetInnerResult<List<OrderDto>, ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
