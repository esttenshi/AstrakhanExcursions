using AstrakhanExcursions.Models;
using Microsoft.EntityFrameworkCore;

namespace AstrakhanExcursions.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TourTag> TourTags { get; set; }
        public DbSet<Models.Route> Routes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Определение составного ключа для TourTag
            modelBuilder.Entity<TourTag>()
                .HasKey(tt => new { tt.TourId, tt.TagId }); // Определение составного ключа
            
            modelBuilder.Entity<Place>().Property(p => p.Latitude)
                .HasColumnType("decimal(9, 6)"); // Укажите тип и точность           

            modelBuilder.Entity<Place>()
                .Property(p => p.Longitude)
                .HasColumnType("decimal(9, 6)"); // Укажите тип и точность
        }
    }
}
