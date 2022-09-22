using Microsoft.EntityFrameworkCore;
using payobills.bills.models;

namespace payobills.bills.data;

public class BillsContext : DbContext
{
    public BillsContext(DbContextOptions<BillsContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        this.Database.EnsureCreated();
    }

    public DbSet<Bill> Bills { get; set; } = default!;
}