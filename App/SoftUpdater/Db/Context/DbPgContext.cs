using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUpdater.Db.Attributes;
using SoftUpdater.Db.Model;
using System;
using System.Reflection;


namespace SoftUpdater.Db.Context
{
    

    public class DbPgContext : DbContext
    {        
        public DbSet<Settings> Settings { get; set; }

        public DbPgContext(DbContextOptions<DbPgContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresExtension("uuid-ossp");
            //Database.EnsureCreated();

            modelBuilder.ApplyConfiguration(new EntityConfiguration<Settings>());

            foreach (var type in Assembly.GetAssembly(typeof(Entity)).GetTypes())
            {
                if (typeof(Entity).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    var configType = typeof(EntityConfiguration<>).MakeGenericType(type);
                    var config = Activator.CreateInstance(configType);
                    GetType().GetMethod(nameof(ApplyConf), BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(type).Invoke(this, new object[] { modelBuilder, config });


                }
            }
        }

        private void ApplyConf<T>(ModelBuilder modelBuilder, EntityConfiguration<T> config) where T : Entity
        {
            modelBuilder.ApplyConfiguration(config);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }
    }

    public class EntityConfiguration<T> : IEntityTypeConfiguration<T>
        where T : class
    {
        public void Configure(EntityTypeBuilder<T> builder)
        {
            var type = typeof(T);
            var typeAttribute = type.GetCustomAttribute<TableNameAttribute>();
            if (typeAttribute != null)
            {
                builder.ToTable(typeAttribute.Name);
            }
            else
            {
                builder.ToTable(type.Name);
            }

            foreach (var prop in type.GetProperties())
            {
                var ignore = prop.GetCustomAttribute<IgnoreAttribute>();
                if (ignore == null)
                {
                    var pkAttr = prop.GetCustomAttribute<PrimaryKeyAttribute>();
                    if (pkAttr != null)
                    {
                        builder.HasKey(prop.Name);
                    }

                    var propAttribute = prop.GetCustomAttribute<ColumnNameAttribute>();
                    if (propAttribute != null)
                        builder.Property(prop.Name)
                            .HasColumnName(propAttribute.Name);
                    else
                        builder.Property(prop.Name)
                            .HasColumnName(prop.Name);

                    var ctAttr = prop.GetCustomAttribute<ColumnTypeAttribute>();
                    if (ctAttr != null)
                    {
                        builder.Property(prop.Name).HasColumnType(ctAttr.Name);
                    }
                }
                else
                {
                    builder.Ignore(prop.Name);
                }
            }
        }
    }
}
