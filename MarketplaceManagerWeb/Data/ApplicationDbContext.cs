using MarketplaceManagerWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceManagerWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Manager> Managers { get; set; }
        public DbSet<Marketplace> Marketplaces { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
    }
}