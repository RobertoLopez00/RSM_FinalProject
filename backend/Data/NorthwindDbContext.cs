using Microsoft.EntityFrameworkCore;
using backend.Entities;

namespace backend.Data;

public class NorthwindDbContext : DbContext
{
    public NorthwindDbContext(DbContextOptions<NorthwindDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Shipper> Shippers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer configuration
        modelBuilder.Entity<Customer>()
            .HasKey(c => c.CustomerID);

        // Employee configuration
        modelBuilder.Entity<Employee>()
            .HasKey(e => e.EmployeeID);

        // Shipper configuration
        modelBuilder.Entity<Shipper>()
            .HasKey(s => s.ShipperID);

        // Product configuration
        modelBuilder.Entity<Product>()
            .HasKey(p => p.ProductID);

        // Order configuration
        modelBuilder.Entity<Order>()
            .HasKey(o => o.OrderID);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerID)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Employee)
            .WithMany(e => e.Orders)
            .HasForeignKey(o => o.EmployeeID)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Shipper)
            .WithMany(s => s.Orders)
            .HasForeignKey(o => o.ShipVia)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // OrderDetail configuration (composite key)
        modelBuilder.Entity<OrderDetail>()
            .HasKey(od => new { od.OrderID, od.ProductID });

        modelBuilder.Entity<OrderDetail>()
            .ToTable("Order Details");

        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Product)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(od => od.ProductID)
            .OnDelete(DeleteBehavior.Restrict);

        // Property configuration
        modelBuilder.Entity<Product>()
            .Property(p => p.UnitPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.Freight)
            .HasPrecision(10, 2);

        modelBuilder.Entity<OrderDetail>()
            .Property(od => od.UnitPrice)
            .HasPrecision(10, 2);
    }
}
