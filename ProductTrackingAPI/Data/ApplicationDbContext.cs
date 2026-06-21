using ProductTrackingAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductTrackingAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<SaleOrder> SaleOrders => Set<SaleOrder>();

    public DbSet<SaleOrderTransportDetail> SaleOrderTransportDetails
        => Set<SaleOrderTransportDetail>();

    public DbSet<SaleOrderDeliveryDetail> SaleOrderDeliveryDetails
        => Set<SaleOrderDeliveryDetail>();

    public DbSet<HuItem> HuItems => Set<HuItem>();

    public DbSet<ScanTransaction> ScanTransactions
        => Set<ScanTransaction>();

    public DbSet<MismatchLog> MismatchLogs
        => Set<MismatchLog>();

    public DbSet<EmailLog> EmailLogs
        => Set<EmailLog>();

    public DbSet<VehicleDispatch> VehicleDispatches
        => Set<VehicleDispatch>();

    public DbSet<AuditLog> AuditLogs { get; set; }

    public DbSet<ExceptionLog> ExceptionLogs { get; set; }

    protected override void OnModelCreating(
     ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new
            {
                ur.UserId,
                ur.RoleId
            });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        modelBuilder.Entity<ScanTransaction>()
            .HasOne(st => st.User)
            .WithMany()
            .HasForeignKey(st => st.ScannedBy);

        modelBuilder.Entity<ScanTransaction>()
            .HasOne(st => st.HuItem)
            .WithMany()
            .HasForeignKey(st => st.HuItemId);

        modelBuilder.Entity<HuItem>()
    .HasOne(h => h.SaleOrder)
    .WithMany()
    .HasForeignKey(h => h.SaleOrderId);
    }
}