using Microsoft.EntityFrameworkCore;
using EShop.Domain.Customers;
using EShop.Domain.Products;
using EShop.Domain.Orders;
using EShop.Domain.Auth;

namespace EShop.Infrastructure.Persistence;

/// <summary>
/// main db context with sqlite, configures all aggregates and value objects
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<CsvImportRecord> CsvImports => Set<CsvImportRecord>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // customers
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id)
                .HasConversion(id => id.Value, value => new CustomerId(value));

            entity.Property(c => c.Email)
                .HasConversion(email => email.Value, value => Email.Create(value))
                .HasMaxLength(256);
            entity.HasIndex(c => c.Email).IsUnique();

            entity.Property(c => c.Phone)
                .HasConversion(phone => phone.Value, value => Phone.Create(value))
                .HasMaxLength(20);

            entity.Property(c => c.FirstName).HasMaxLength(100);
            entity.Property(c => c.LastName).HasMaxLength(100);

            entity.Ignore(c => c.DomainEvents);
        });

        // addresses
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Line1).HasMaxLength(500);
            entity.Property(a => a.City).HasMaxLength(100);
            entity.Property(a => a.Country).HasMaxLength(100);
            entity.Property(a => a.Type)
                .HasConversion<string>();
        });

        // products
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id)
                .HasConversion(id => id.Value, value => new ProductId(value));

            entity.Property(p => p.Name).HasMaxLength(500);
            entity.HasIndex(p => p.Name);

            entity.Property(p => p.Price)
                .HasConversion(
                    money => money.Amount,
                    value => Money.Create(value, "USD"))
                .HasPrecision(18, 2);

            entity.Property(p => p.Sku)
                .HasConversion(sku => sku.Value, value => Sku.Create(value))
                .HasMaxLength(50);
            entity.HasIndex(p => p.Sku).IsUnique();

            entity.Property(p => p.ManufacturedFrom).HasMaxLength(100);
            entity.Property(p => p.ShippedFrom).HasMaxLength(100);

            entity.Ignore(p => p.DomainEvents);
        });

        // orders
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id)
                .HasConversion(id => id.Value, value => new OrderId(value));

            entity.Property(o => o.CustomerId)
                .HasConversion(id => id.Value, value => new CustomerId(value));

            entity.Property(o => o.Status)
                .HasConversion<string>();

            entity.Property(o => o.TrackingNumber)
                .HasConversion(tn => tn.Value, value => TrackingNumber.Create(value))
                .HasMaxLength(20);

            entity.Property(o => o.Total)
                .HasConversion(
                    money => money.Amount,
                    value => Money.Create(value, "USD"))
                .HasPrecision(18, 2);

            entity.Ignore(o => o.DomainEvents);
        });

        // order items
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);

            entity.Property(oi => oi.OrderId)
                .HasConversion(id => id.Value, value => new OrderId(value));

            entity.Property(oi => oi.ProductId)
                .HasConversion(id => id.Value, value => new ProductId(value));

            entity.Property(oi => oi.UnitPrice)
                .HasConversion(
                    money => money.Amount,
                    value => Money.Create(value, "USD"))
                .HasPrecision(18, 2);

            entity.Property(oi => oi.TotalPrice)
                .HasConversion(
                    money => money.Amount,
                    value => Money.Create(value, "USD"))
                .HasPrecision(18, 2);
        });

        // user accounts
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id)
                .HasConversion(id => id.Value, value => new UserAccountId(value));

            entity.Property(u => u.Email)
                .HasConversion(email => email.Value, value => Email.Create(value))
                .HasMaxLength(256);
            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.Role)
                .HasConversion<string>();

            entity.Property(u => u.CustomerId)
                .HasConversion(
                    id => id!.Value,
                    value => new CustomerId(value));

            entity.HasMany(u => u.RefreshTokens)
                .WithOne()
                .HasForeignKey(t => t.UserAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(u => u.DomainEvents);
        });

        // refresh tokens
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.UserAccountId)
                .HasConversion(id => id.Value, value => new UserAccountId(value));

            entity.Property(t => t.Token).HasMaxLength(500);
            entity.HasIndex(t => t.Token);
        });

        // csv imports
        modelBuilder.Entity<CsvImportRecord>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Checksum).HasMaxLength(64);
        });
    }
}

/// <summary>
/// tracks csv imports to make them idempotent
/// </summary>
public class CsvImportRecord
{
    public Guid Id { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public DateTime ImportedAt { get; set; }
}
