using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using kriefTrackAiApi.Core.Models;

namespace kriefTrackAiApi.Infrastructure.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(c => c.Id); // Primary Key
        builder.Property(c => c.Id)
                 .HasDefaultValueSql("uuid_generate_v4()")
                 .ValueGeneratedOnAdd();
        builder.Property(c => c.CustomerName).IsRequired().HasMaxLength(200);
    }
}
