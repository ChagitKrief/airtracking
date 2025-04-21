using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using kriefTrackAiApi.Core.Models;

namespace kriefTrackAiApi.Infrastructure.Data.Configurations;

public class ShipmentCustomersConfiguration : IEntityTypeConfiguration<ShipmentCustomers>
{
    public void Configure(EntityTypeBuilder<ShipmentCustomers> builder)
    {
        builder.HasKey(c => c.Id); // Primary Key
        builder.Property(c => c.Id)
                 .HasDefaultValueSql("uuid_generate_v4()")
                 .ValueGeneratedOnAdd();
    }
}
