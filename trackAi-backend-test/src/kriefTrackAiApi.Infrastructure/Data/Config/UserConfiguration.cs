using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using kriefTrackAiApi.Core.Models;

namespace kriefTrackAiApi.Infrastructure.Data.Configurations;

     public class UserConfiguration : IEntityTypeConfiguration<User>
     {
            public void Configure(EntityTypeBuilder<User> builder)
            {
                   builder.HasKey(u => u.Id);

                   builder.Property(u => u.Id)
                          .HasDefaultValueSql("uuid_generate_v4()")
                          .ValueGeneratedOnAdd();

                   builder.Property(u => u.Email)
                          .IsRequired()
                          .HasMaxLength(150);

                   builder.Property(u => u.Password)
                          .IsRequired();

                   builder.Property(u => u.Reminders)
                 .HasColumnType("text[]"); // PostgreSQL Array

                   builder.Property(u => u.CompanyIds)
                          .HasColumnType("uuid[]");
            }
     }
