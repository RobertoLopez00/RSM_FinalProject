namespace backend.DTOs;

public class OrderDetailDto
{
    public int OrderID { get; set; }
    public int ProductID { get; set; }
    public string? ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public short Quantity { get; set; }
    public float Discount { get; set; }
    public decimal LineTotal { get; set; }
}

public class CreateOrderDetailDto
{
    public int ProductID { get; set; }
    public decimal UnitPrice { get; set; }
    public short Quantity { get; set; }
    public float Discount { get; set; }
}
