using Microsoft.EntityFrameworkCore;
using ProductApp.Models;

namespace ProductApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<SiteSetting> SiteSettings { get; set; }

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<ProfileTab> ProfileTabs { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure one-to-one relationship between User and Profile
            modelBuilder.Entity<User>()
                .HasOne(u => u.ProfProfileTabile)
                .WithOne(p => p.User)
                .HasForeignKey<ProfileTab>(p => p.UserId);

            // You might want to ensure UserId is unique for one-to-one relationship
            modelBuilder.Entity<ProfileTab>()
                .HasIndex(p => p.UserId)
                .IsUnique();
        }
    }

    
}
