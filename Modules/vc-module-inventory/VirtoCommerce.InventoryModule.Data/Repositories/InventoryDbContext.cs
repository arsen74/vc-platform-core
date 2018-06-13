using System;
using System.Collections.Generic;
using System.Text;
using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.InventoryModule.Data.Model;

namespace VirtoCommerce.InventoryModule.Data.Repositories
{
    public class InventoryDbContext : DbContextWithTriggers
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InventoryEntity>().ToTable("Inventory").HasKey(x => x.Id);
            modelBuilder.Entity<InventoryEntity>().Property(x => x.Id).HasMaxLength(128);

            modelBuilder.Entity<FulfillmentCenterEntity>().ToTable("FulfillmentCenter").HasKey(x => x.Id);
            modelBuilder.Entity<FulfillmentCenterEntity>().Property(x => x.Id).HasMaxLength(128);
            modelBuilder.Entity<InventoryEntity>().HasOne(x => x.FulfillmentCenter).WithMany()
                .HasForeignKey(x => x.FulfillmentCenterId)
                .OnDelete(DeleteBehavior.SetNull);


            base.OnModelCreating(modelBuilder);
        }
    }
}
