using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using kriefTrackAiApi.Core.Models;
using System.Text.Json;

namespace kriefTrackAiApi.Infrastructure.Data.Configurations;

public class SmsConfiguration : IEntityTypeConfiguration<Sms>
{
    public void Configure(EntityTypeBuilder<Sms> builder)
    {
        builder.HasKey(sms => sms.Id);

        builder.Property(sms => sms.Id)
               .HasDefaultValueSql("uuid_generate_v4()")
               .ValueGeneratedOnAdd();

        builder.Property(sms => sms.Container)
               .IsRequired();

        builder.OwnsMany(sms => sms.UserPhones, userPhoneBuilder =>
        {
            userPhoneBuilder.WithOwner().HasForeignKey("SmsId");

            userPhoneBuilder.Property(up => up.UserId)
                            .IsRequired();

            userPhoneBuilder.Property(up => up.Phones)
       .HasConversion(
           v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
           v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
       )
       .HasColumnType("jsonb");

        });
    }
}
