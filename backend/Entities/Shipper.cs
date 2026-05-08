namespace backend.Entities;

public class Shipper
{
    public int ShipperID { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Phone { get; set; }

    // Navigation
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
