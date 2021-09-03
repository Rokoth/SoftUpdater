using DbClient.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbClient.Context
{
    public class DbSqLiteContext : DbContext
    {       
        public DbSet<Settings> Settings { get; set; }

        public DbSqLiteContext(DbContextOptions<DbSqLiteContext> options) : base(options) {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            modelBuilder.ApplyConfiguration(new SettingsEntityConfiguration());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.EnableSensitiveDataLogging(true);
        }
    }

    public class SettingsEntityConfiguration : IEntityTypeConfiguration<Settings>
    {
        public void Configure(EntityTypeBuilder<Settings> builder)
        {
            builder.ToTable("settings");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.ParamName).HasColumnName("param_name");
            builder.Property(s => s.ParamValue).HasColumnName("param_value");
        }
    }
}
