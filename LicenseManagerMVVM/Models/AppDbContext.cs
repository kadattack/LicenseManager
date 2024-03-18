using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace LicenseManagerMVVM.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<License> License { get; set; }
        public DbSet<Client> Client { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        
        public override int SaveChanges()
        {
            ConvertDateTimePropertiesToUtc();
            return base.SaveChanges();
        }

        private void ConvertDateTimePropertiesToUtc()
        {
            var entities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entity in entities)
            {
                foreach (var property in entity.Properties)
                {
                    if (property.Metadata.ClrType == typeof(DateTime))
                    {
                        var currentValue = (DateTime)property.CurrentValue;
                        if (currentValue.Kind != DateTimeKind.Utc)
                        {
                            property.CurrentValue = currentValue.ToUniversalTime();
                        }
                    }
                }
            }
        }

        // Additional configurations...

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional configurations...
        }
    }
}