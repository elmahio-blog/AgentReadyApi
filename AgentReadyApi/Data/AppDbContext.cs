using AgentReadyApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentReadyApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CustomerId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(x => x.Amount)
                .HasPrecision(18, 2);

            entity.Property(x => x.Status)
                .HasConversion<string>();

            entity.Property(x => x.CreatedAt)
                .IsRequired();
        });
    }
}