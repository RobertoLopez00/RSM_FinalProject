using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using backend.Services;
using backend.Entities;
using backend.Data;
using backend.DTOs;

namespace backend.Tests;

public class OrderServiceTests
{
    private NorthwindDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<NorthwindDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new NorthwindDbContext(options);
    }

    private void SeedTestData(NorthwindDbContext dbContext)
    {
        var customer = new Customer { CustomerID = "CUST1", CompanyName = "Test Company", City = "NY", Country = "USA" };
        var employee = new Employee { EmployeeID = 1, FirstName = "Jane", LastName = "Smith", Title = "Sales" };
        var shipper = new Shipper { ShipperID = 1, CompanyName = "Fast Shipping" };
        var product = new Product { ProductID = 1, ProductName = "Test Product", UnitPrice = 18.00m, UnitsInStock = 100 };
        
        dbContext.Customers.Add(customer);
        dbContext.Employees.Add(employee);
        dbContext.Shippers.Add(shipper);
        dbContext.Products.Add(product);
        dbContext.SaveChanges();
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidData_CreatesOrder()
    {
        var dbContext = CreateInMemoryDbContext();
        SeedTestData(dbContext);
        var service = new OrderService(dbContext);

        var createDto = new CreateOrderDto
        {
            CustomerID = "CUST1",
            EmployeeID = 1,
            OrderDate = DateTime.Now,
            ShipVia = 1,
            Freight = 50.00m,
            ShipName = "Test",
            ShipAddress = "123 Main",
            ShipCity = "NY",
            ShipCountry = "USA",
            OrderDetails = new List<CreateOrderDetailDto> { new CreateOrderDetailDto { ProductID = 1, Quantity = 5, UnitPrice = 18.00m, Discount = 0 } }
        };

        var result = await service.CreateOrderAsync(createDto);

        Assert.NotNull(result);
        Assert.True(result.OrderID > 0);
        Assert.Equal("CUST1", result.CustomerID);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ReturnsAllOrders()
    {
        var dbContext = CreateInMemoryDbContext();
        SeedTestData(dbContext);
        var order = new Order { OrderID = 10001, CustomerID = "CUST1", EmployeeID = 1, OrderDate = DateTime.Now, ShipVia = 1, Freight = 50.00m, ShipName = "Test", ShipAddress = "123 Main", ShipCity = "NY", ShipCountry = "USA" };
        dbContext.Orders.Add(order);
        dbContext.SaveChanges();

        var service = new OrderService(dbContext);
        var result = await service.GetAllOrdersAsync();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(10001, result[0].OrderID);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithValidId_ReturnsOrder()
    {
        var dbContext = CreateInMemoryDbContext();
        SeedTestData(dbContext);
        var order = new Order { OrderID = 10002, CustomerID = "CUST1", EmployeeID = 1, OrderDate = DateTime.Now, ShipVia = 1, Freight = 50.00m, ShipName = "Test", ShipAddress = "123 Main", ShipCity = "NY", ShipCountry = "USA" };
        dbContext.Orders.Add(order);
        dbContext.SaveChanges();

        var service = new OrderService(dbContext);
        var result = await service.GetOrderByIdAsync(10002);

        Assert.NotNull(result);
        Assert.Equal(10002, result.OrderID);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithShippedDateInPast_ReturnsShippedStatus()
    {
        var dbContext = CreateInMemoryDbContext();
        SeedTestData(dbContext);
        var shippedDate = DateTime.UtcNow.AddDays(-1);
        var order = new Order { OrderID = 10009, CustomerID = "CUST1", EmployeeID = 1, OrderDate = DateTime.UtcNow.AddDays(-5), RequiredDate = DateTime.UtcNow.AddDays(2), ShippedDate = shippedDate, ShipVia = 1, Freight = 50.00m, ShipName = "Test", ShipAddress = "123 Main", ShipCity = "NY", ShipCountry = "USA" };
        dbContext.Orders.Add(order);
        dbContext.SaveChanges();

        var service = new OrderService(dbContext);
        var result = await service.GetOrderByIdAsync(10009);

        Assert.NotNull(result);
        Assert.Equal("Shipped", result.Status);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithFutureShippedDate_ReturnsShippedStatus()
    {
        var dbContext = CreateInMemoryDbContext();
        SeedTestData(dbContext);
        var shippedDate = DateTime.UtcNow.AddDays(2);
        var order = new Order { OrderID = 10010, CustomerID = "CUST1", EmployeeID = 1, OrderDate = DateTime.UtcNow, RequiredDate = DateTime.UtcNow.AddDays(10), ShippedDate = shippedDate, ShipVia = 1, Freight = 50.00m, ShipName = "Test", ShipAddress = "123 Main", ShipCity = "NY", ShipCountry = "USA" };
        dbContext.Orders.Add(order);
        dbContext.SaveChanges();

        var service = new OrderService(dbContext);
        var result = await service.GetOrderByIdAsync(10010);

        Assert.NotNull(result);
        Assert.Equal("Shipped", result.Status);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithInvalidId_ReturnsNull()
    {
        var dbContext = CreateInMemoryDbContext();
        SeedTestData(dbContext);
        var service = new OrderService(dbContext);

        var result = await service.GetOrderByIdAsync(99999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateOrderAsync_WithValidData_UpdatesOrder()
    {
        var dbContext = CreateInMemoryDbContext();
        SeedTestData(dbContext);
        var order = new Order { OrderID = 10003, CustomerID = "CUST1", EmployeeID = 1, OrderDate = DateTime.Now, ShipVia = 1, Freight = 50.00m, ShipName = "Old", ShipAddress = "123 Main", ShipCity = "NY", ShipCountry = "USA" };
        dbContext.Orders.Add(order);
        dbContext.SaveChanges();

        var service = new OrderService(dbContext);
        var updateDto = new UpdateOrderDto { ShipName = "New Address" };
        var result = await service.UpdateOrderAsync(10003, updateDto);

        Assert.NotNull(result);
        Assert.Equal("New Address", result.ShipName);
    }

    [Fact]
    public async Task UpdateOrderAsync_WithOrderDate_UpdatesOrderDate()
    {
        var dbContext = CreateInMemoryDbContext();
        SeedTestData(dbContext);
        var originalDate = new DateTime(2023, 1, 1);
        var newDate = new DateTime(2023, 12, 25);
        var order = new Order { OrderID = 10004, CustomerID = "CUST1", EmployeeID = 1, OrderDate = originalDate, ShipVia = 1, Freight = 50.00m, ShipName = "Test", ShipAddress = "123 Main", ShipCity = "NY", ShipCountry = "USA" };
        dbContext.Orders.Add(order);
        dbContext.SaveChanges();

        var service = new OrderService(dbContext);
        var updateDto = new UpdateOrderDto { OrderDate = newDate };
        var result = await service.UpdateOrderAsync(10004, updateDto);

        Assert.NotNull(result);
        Assert.Equal(newDate, result.OrderDate);
    }

    [Fact]
    public async Task UpdateOrderAsync_WithClearDateFlags_ClearsNullableDates()
    {
        var dbContext = CreateInMemoryDbContext();
        SeedTestData(dbContext);
        var order = new Order
        {
            OrderID = 10011,
            CustomerID = "CUST1",
            EmployeeID = 1,
            OrderDate = DateTime.Now,
            RequiredDate = DateTime.Now.AddDays(5),
            ShippedDate = DateTime.Now.AddDays(1),
            ShipVia = 1,
            Freight = 50.00m,
            ShipName = "Test",
            ShipAddress = "123 Main",
            ShipCity = "NY",
            ShipCountry = "USA"
        };
        dbContext.Orders.Add(order);
        dbContext.SaveChanges();

        var service = new OrderService(dbContext);
        var updateDto = new UpdateOrderDto { ClearRequiredDate = true, ClearShippedDate = true };
        var result = await service.UpdateOrderAsync(10011, updateDto);

        Assert.NotNull(result);
        Assert.Null(result.RequiredDate);
        Assert.Null(result.ShippedDate);
    }

    [Fact]
    public async Task UpdateOrderAsync_WithInvalidId_ThrowsException()
    {
        var dbContext = CreateInMemoryDbContext();
        SeedTestData(dbContext);
        var service = new OrderService(dbContext);
        var updateDto = new UpdateOrderDto { ShipName = "New" };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateOrderAsync(99999, updateDto));
    }

    [Fact]
    public async Task DeleteOrderAsync_WithValidId_DeletesOrder()
    {
        var dbContext = CreateInMemoryDbContext();
        SeedTestData(dbContext);
        var order = new Order { OrderID = 10004, CustomerID = "CUST1", EmployeeID = 1, OrderDate = DateTime.Now, ShipVia = 1, Freight = 50.00m, ShipName = "Test", ShipAddress = "123 Main", ShipCity = "NY", ShipCountry = "USA" };
        dbContext.Orders.Add(order);
        dbContext.SaveChanges();

        var service = new OrderService(dbContext);
        var result = await service.DeleteOrderAsync(10004);
        var deleted = await service.GetOrderByIdAsync(10004);

        Assert.True(result);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteOrderAsync_WithInvalidId_ReturnsFalse()
    {
        var dbContext = CreateInMemoryDbContext();
        SeedTestData(dbContext);
        var service = new OrderService(dbContext);

        var result = await service.DeleteOrderAsync(99999);

        Assert.False(result);
    }

    [Fact]
    public async Task GetOrdersByCustomerAsync_ReturnsOrdersForCustomer()
    {
        var dbContext = CreateInMemoryDbContext();
        SeedTestData(dbContext);
        var order1 = new Order { OrderID = 10005, CustomerID = "CUST1", EmployeeID = 1, OrderDate = DateTime.Now, ShipVia = 1, Freight = 50.00m, ShipName = "Test1", ShipAddress = "123 Main", ShipCity = "NY", ShipCountry = "USA" };
        var order2 = new Order { OrderID = 10006, CustomerID = "OTHER", EmployeeID = 1, OrderDate = DateTime.Now, ShipVia = 1, Freight = 50.00m, ShipName = "Test2", ShipAddress = "456 Oak", ShipCity = "LA", ShipCountry = "USA" };
        dbContext.Orders.AddRange(order1, order2);
        dbContext.SaveChanges();

        var service = new OrderService(dbContext);
        var result = await service.GetOrdersByCustomerAsync("CUST1");

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("CUST1", result[0].CustomerID);
    }

    [Fact]
    public async Task GetOrdersByDateRangeAsync_ReturnsOrdersInRange()
    {
        var dbContext = CreateInMemoryDbContext();
        SeedTestData(dbContext);
        var date1 = new DateTime(2026, 04, 01);
        var date2 = new DateTime(2026, 04, 15);
        var date3 = new DateTime(2026, 05, 01);

        var order1 = new Order { OrderID = 10007, CustomerID = "CUST1", EmployeeID = 1, OrderDate = date1, ShipVia = 1, Freight = 50.00m, ShipName = "Test1", ShipAddress = "123 Main", ShipCity = "NY", ShipCountry = "USA" };
        var order2 = new Order { OrderID = 10008, CustomerID = "CUST1", EmployeeID = 1, OrderDate = date3, ShipVia = 1, Freight = 50.00m, ShipName = "Test2", ShipAddress = "456 Oak", ShipCity = "LA", ShipCountry = "USA" };
        dbContext.Orders.AddRange(order1, order2);
        dbContext.SaveChanges();

        var service = new OrderService(dbContext);
        var result = await service.GetOrdersByDateRangeAsync(date1, date2);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(10007, result[0].OrderID);
    }

    [Fact]
    public async Task CreateOrderAsync_WithMissingProduct_ThrowsException()
    {
        var dbContext = CreateInMemoryDbContext();
        SeedTestData(dbContext);
        var service = new OrderService(dbContext);

        var createDto = new CreateOrderDto
        {
            CustomerID = "CUST1",
            EmployeeID = 1,
            OrderDate = DateTime.Now,
            ShipVia = 1,
            Freight = 50.00m,
            ShipName = "Test",
            ShipAddress = "123 Main",
            ShipCity = "NY",
            ShipCountry = "USA",
            OrderDetails = new List<CreateOrderDetailDto> { new CreateOrderDetailDto { ProductID = 999, Quantity = 5, UnitPrice = 18.00m, Discount = 0 } }
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateOrderAsync(createDto));
    }
}
