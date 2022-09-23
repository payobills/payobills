using Microsoft.EntityFrameworkCore;
using payobills.bills.models;

namespace payobills.bills.data;

public class BillsContext : DbContext
{
    public BillsContext(DbContextOptions<BillsContext> options) : base(options) { }

    public DbSet<Bill> Bills { get; set; } = default!;
}