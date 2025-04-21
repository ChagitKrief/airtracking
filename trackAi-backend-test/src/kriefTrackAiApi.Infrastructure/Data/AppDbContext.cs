using Microsoft.EntityFrameworkCore;
using kriefTrackAiApi.Core.Models;
using kriefTrackAiApi.Core.Interfaces;

namespace kriefTrackAiApi.Infrastructure.Data;

public class AppDbContext : DbContext, IContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Sms> SmsMessages { get; set; }
    public DbSet<ShipmentCustomers> ShipmentCustomers { get; set; }
    public async Task Save() => await SaveChangesAsync();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // modelBuilder.Ignore<Company>();
        // modelBuilder.Ignore<User>();
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<Company>().ToTable("companies");
        modelBuilder.Entity<ShipmentCustomers>().ToTable("shipmentcustomers");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        // modelBuilder.Entity<Company>()
        //         .ToTable("companies", t => t.ExcludeFromMigrations());
        //         modelBuilder.Entity<User>()
        //         .ToTable("Users", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<Sms>(entity =>
        {
            entity.ToTable("smsmessages");

            entity.Property(e => e.UserPhonesJson)
                  .HasColumnType("jsonb")
                  .HasColumnName("userphones");
            entity.Ignore(e => e.UserPhones);
        });
    }
}
