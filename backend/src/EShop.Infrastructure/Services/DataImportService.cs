using System.Globalization;
using System.Security.Cryptography;
using EShop.Domain.Auth;
using EShop.Domain.Customers;
using EShop.Domain.Orders;
using EShop.Domain.Products;
using EShop.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace EShop.Infrastructure.Services;

/// <summary>
/// import dataset with deduplication for initial seeding of
/// customers, products, orders, and admin accounts.
/// </summary>
public sealed class DataImportService
{
    private const string DEFAULT_DATA_PATH = "../../data/dataset.xlsx";
    private const string UNKNOWN_ORIGIN = "Unknown";
    private const string DEFAULT_PHONE = "0000000000";
    private const int DATA_START_ROW = 2;

    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<DataImportService> _logger;

    public DataImportService(AppDbContext context, IConfiguration config, ILogger<DataImportService> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    public async Task ImportIfNeededAsync(CancellationToken ct = default)
    {
        ConfigureEPPlus();

        var dataPath = _config["DataImportPath"] ?? DEFAULT_DATA_PATH;
        if (!ValidateDataFile(dataPath))
            return;

        var checksum = await ComputeChecksumAsync(dataPath);
        if (await IsAlreadyImportedAsync(checksum))
        {
            _logger.LogInformation("Data already imported, skipping");
            return;
        }

        _logger.LogInformation("Starting data import from {Path}", dataPath);

        await ImportDataAsync(dataPath, ct);
        await RecordImportAsync(checksum, ct);

        _logger.LogInformation("Data import completed successfully");
    }

    private static void ConfigureEPPlus()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    private bool ValidateDataFile(string dataPath)
    {
        if (File.Exists(dataPath))
            return true;

        _logger.LogWarning("Data file not found at {Path}, skipping import", dataPath);
        return false;
    }

    private async Task<bool> IsAlreadyImportedAsync(string checksum)
    {
        var existingImport = _context.CsvImports.FirstOrDefault(i => i.Checksum == checksum);
        if (existingImport != null)
        {
            _logger.LogInformation("Data with checksum {Checksum} already imported on {Date}",
                checksum, existingImport.ImportedAt);
            return true;
        }
        return false;
    }

    private async Task RecordImportAsync(string checksum, CancellationToken ct)
    {
        _context.CsvImports.Add(new DataImportRecord
        {
            Id = Guid.NewGuid(),
            Checksum = checksum,
            ImportedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(ct);
    }

    private static async Task<string> ComputeChecksumAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var hash = await SHA256.HashDataAsync(stream);
        return Convert.ToHexString(hash);
    }

    private async Task ImportDataAsync(string excelPath, CancellationToken ct)
    {
        var records = await ReadExcelRecordsAsync(excelPath);
        _logger.LogInformation("Found {Count} records to import", records.Count);

        var (customers, addresses, products, orders, userAccounts) = TransformRecords(records);

        await PersistEntitiesAsync(customers, addresses, products, orders, userAccounts, ct);

        _logger.LogInformation("Imported {Customers} customers, {Products} products, {Orders} orders",
            customers.Count, products.Count, orders.Count);

        await SeedAdminIfConfiguredAsync(ct);
    }

    private async Task<List<ImportRecord>> ReadExcelRecordsAsync(string excelPath)
    {
        var records = new List<ImportRecord>();

        using var package = new ExcelPackage(new FileInfo(excelPath));
        var worksheet = package.Workbook.Worksheets[0];
        var rowCount = worksheet.Dimension.Rows;

        for (int row = DATA_START_ROW; row <= rowCount; row++)
        {
            try
            {
                var record = ParseExcelRow(worksheet, row);
                records.Add(record);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse row {Row}", row);
            }
        }

        return records;
    }

    private static ImportRecord ParseExcelRow(ExcelWorksheet worksheet, int row)
    {
        return new ImportRecord
        {
            ID = worksheet.Cells[row, 1].GetValue<int>(),
            FirstName = worksheet.Cells[row, 2].GetValue<string>() ?? string.Empty,
            LastName = worksheet.Cells[row, 3].GetValue<string>() ?? string.Empty,
            ShippingAddress = worksheet.Cells[row, 4].GetValue<string>() ?? string.Empty,
            ShippingCity = worksheet.Cells[row, 5].GetValue<string>() ?? string.Empty,
            ShippingCountry = worksheet.Cells[row, 6].GetValue<string>() ?? string.Empty,
            Card = PaymentCard.CreateMasked(
                worksheet.Cells[row, 7].GetValue<string>(),
                worksheet.Cells[row, 8].GetValue<string>()),
            BillingCity = worksheet.Cells[row, 9].GetValue<string>() ?? string.Empty,
            BillingAddress = worksheet.Cells[row, 10].GetValue<string>() ?? string.Empty,
            BillingCountry = worksheet.Cells[row, 11].GetValue<string>() ?? string.Empty,
            TrackingNumber = worksheet.Cells[row, 12].GetValue<string>(),
            ItemName = worksheet.Cells[row, 13].GetValue<string>() ?? string.Empty,
            PricePerItem = Money.ParseFromString(worksheet.Cells[row, 14].GetValue<string>()),
            PurchaseDate = worksheet.Cells[row, 15].GetValue<string>(),
            EstimatedDelivery = worksheet.Cells[row, 16].GetValue<string>(),
            ItemAmount = worksheet.Cells[row, 17].GetValue<int>(),
            ShippedFrom = worksheet.Cells[row, 18].GetValue<string>(),
            ManufacturedFrom = worksheet.Cells[row, 19].GetValue<string>(),
            Phone = worksheet.Cells[row, 20].GetValue<string>(),
            Email = worksheet.Cells[row, 21].GetValue<string>() ?? string.Empty
        };
    }

    private (Dictionary<string, Customer> Customers,
             Dictionary<string, Address> Addresses,
             Dictionary<string, Product> Products,
             List<Order> Orders,
             Dictionary<string, UserAccount> UserAccounts) TransformRecords(List<ImportRecord> records)
    {
        var customerDict = new Dictionary<string, Customer>();
        var addressDict = new Dictionary<string, Address>();
        var productDict = new Dictionary<string, Product>();
        var userAccountDict = new Dictionary<string, UserAccount>();
        var orders = new List<Order>();

        foreach (var record in records)
        {
            try
            {
                var (customer, userAccount) = GetOrCreateCustomerWithAccount(record, customerDict, userAccountDict);
                var shippingAddress = GetOrCreateAddress(record, customer, AddressType.Shipping, addressDict);
                var billingAddress = GetOrCreateAddress(record, customer, AddressType.Billing, addressDict);
                var product = GetOrCreateProduct(record, productDict);
                var order = CreateOrder(record, customer, shippingAddress, billingAddress, product);

                orders.Add(order);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to import record for {Email}", record.Email);
            }
        }

        return (customerDict, addressDict, productDict, orders, userAccountDict);
    }

    private static (Customer, UserAccount) GetOrCreateCustomerWithAccount(
        ImportRecord record,
        Dictionary<string, Customer> customerDict,
        Dictionary<string, UserAccount> userAccountDict)
    {
        var email = Email.Create(record.Email);

        if (customerDict.TryGetValue(email.Value, out var existingCustomer) &&
            userAccountDict.TryGetValue(email.Value, out var existingAccount))
        {
            return (existingCustomer, existingAccount);
        }

        var customer = new Customer(
            CustomerId.New(),
            record.FirstName,
            record.LastName,
            Phone.Create(record.Phone ?? DEFAULT_PHONE)
        );

        var userAccount = new UserAccount(
            UserAccountId.New(),
            email,
            UserRole.User,
            customer.Id
        );

        customerDict[email.Value] = customer;
        userAccountDict[email.Value] = userAccount;

        return (customer, userAccount);
    }

    private static Address GetOrCreateAddress(
        ImportRecord record,
        Customer customer,
        AddressType type,
        Dictionary<string, Address> addressDict)
    {
        var (line1, city, country) = type == AddressType.Shipping
            ? (record.ShippingAddress, record.ShippingCity, record.ShippingCountry)
            : (record.BillingAddress, record.BillingCity, record.BillingCountry);

        var key = $"{line1.Trim().ToLowerInvariant()}|{city.Trim().ToLowerInvariant()}|{country.Trim().ToLowerInvariant()}";

        if (addressDict.TryGetValue(key, out var existingAddress))
            return existingAddress;

        var address = new Address(
            Guid.NewGuid(),
            line1,
            city,
            country,
            type,
            customer.Id.Value
        );

        addressDict[key] = address;
        return address;
    }

    private static Product GetOrCreateProduct(ImportRecord record, Dictionary<string, Product> productDict)
    {
        var sku = Sku.Generate(record.ItemName, record.ManufacturedFrom ?? UNKNOWN_ORIGIN);
        var key = sku.Value;

        if (productDict.TryGetValue(key, out var existingProduct))
            return existingProduct;

        var product = new Product(
            ProductId.New(),
            record.ItemName,
            record.PricePerItem,
            sku,
            record.ManufacturedFrom ?? UNKNOWN_ORIGIN,
            record.ShippedFrom ?? UNKNOWN_ORIGIN
        );

        productDict[key] = product;
        return product;
    }

    private static Order CreateOrder(
        ImportRecord record,
        Customer customer,
        Address shippingAddress,
        Address billingAddress,
        Product product)
    {
        var trackingNumber = string.IsNullOrWhiteSpace(record.TrackingNumber)
            ? TrackingNumber.Generate(record.ShippingCountry)
            : TrackingNumber.Create(record.TrackingNumber);

        var order = new Order(
            OrderId.New(),
            customer.Id,
            trackingNumber,
            shippingAddress.Id,
            billingAddress.Id,
            ParseDate(record.PurchaseDate),
            record.Card,
            ParseDate(record.EstimatedDelivery)
        );

        var orderItem = new OrderItem(
            Guid.NewGuid(),
            order.Id,
            product.Id,
            record.ItemAmount,
            product.Price
        );

        order.AddItem(orderItem);
        return order;
    }

    private async Task PersistEntitiesAsync(
        Dictionary<string, Customer> customers,
        Dictionary<string, Address> addresses,
        Dictionary<string, Product> products,
        List<Order> orders,
        Dictionary<string, UserAccount> userAccounts,
        CancellationToken ct)
    {
        await _context.Customers.AddRangeAsync(customers.Values, ct);
        await _context.UserAccounts.AddRangeAsync(userAccounts.Values, ct);
        await _context.Addresses.AddRangeAsync(addresses.Values, ct);
        await _context.Products.AddRangeAsync(products.Values, ct);
        await _context.Orders.AddRangeAsync(orders, ct);

        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedAdminIfConfiguredAsync(CancellationToken ct)
    {
        var adminEmail = _config["AdminEmail"];
        if (string.IsNullOrWhiteSpace(adminEmail))
            return;

        await SeedAdminAsync(Email.Create(adminEmail), ct);
    }

    private async Task SeedAdminAsync(Email email, CancellationToken ct)
    {
        if (_context.UserAccounts.Any(u => u.Email == email))
        {
            _logger.LogInformation("Admin user {Email} already exists", email.Value);
            return;
        }

        var admin = new UserAccount(UserAccountId.New(), email, UserRole.Admin);

        await _context.UserAccounts.AddAsync(admin, ct);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Seeded admin user: {Email}", email.Value);
    }

    private static DateTime ParseDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
            return DateTime.UtcNow;

        return DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : DateTime.UtcNow;
    }
}

/// <summary>
/// Represents a raw import record from the Excel file.
/// </summary>
internal sealed record ImportRecord
{
    public int ID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public PaymentCard? Card { get; set; }
    public string BillingCity { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public string BillingCountry { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public required Money PricePerItem { get; set; }
    public string? PurchaseDate { get; set; }
    public string? EstimatedDelivery { get; set; }
    public int ItemAmount { get; set; }
    public string? ShippedFrom { get; set; }
    public string? ManufacturedFrom { get; set; }
    public string? Phone { get; set; }
    public string Email { get; set; } = string.Empty;
}
