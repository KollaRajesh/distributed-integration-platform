using Microsoft.EntityFrameworkCore;

namespace InvoiceApi.Persistence;

public sealed class InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : DbContext(options)
{
    public DbSet<InvoiceRecord> Invoices => Set<InvoiceRecord>();
}

public sealed class InvoiceRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ContractId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
