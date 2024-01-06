using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;

namespace WebBanHang.Data
{
    public class ApplicationDbContext:IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {

        }
        public DbSet<Brand> brands { get; set; }
        public DbSet<Category> categories { get;set; }
        public DbSet<Product> products { get; set; }
        public DbSet<ImageProduct> images { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<Slide> Slides { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails {  get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ImageProduct>()
                  .HasKey(m => new { m.ProductId, m.Id });
            modelBuilder.Entity<IdentityRole>().Property(x => x.Id).HasMaxLength(50).IsRequired(true);
            modelBuilder.Entity<User>().Property(x=>x.Id).HasMaxLength(50).IsRequired(true);
            modelBuilder.Entity<OrderDetail>().HasKey(m => new { m.IdOrder, m.IdProductt });
            SeedRoles(modelBuilder);
        }
        private static void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
                new IdentityRole() { Name = "User", ConcurrencyStamp = "2", NormalizedName = "User" },
                new IdentityRole() { Name = "HR", ConcurrencyStamp = "3", NormalizedName = "HR" },
                new IdentityRole() { Name = "Customer", ConcurrencyStamp = "4", NormalizedName = "Customer" }
                );
        }

    }
}
