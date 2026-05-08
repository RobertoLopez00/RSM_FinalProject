namespace backend.DTOs;

public class CustomerDto
{
    public string CustomerID { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
}

public class EmployeeDto
{
    public int EmployeeID { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Title { get; set; }
    public string FullName => $"{FirstName} {LastName}";
}

public class ShipperDto
{
    public int ShipperID { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class ProductDto
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal? UnitPrice { get; set; }
    public short? UnitsInStock { get; set; }
}
