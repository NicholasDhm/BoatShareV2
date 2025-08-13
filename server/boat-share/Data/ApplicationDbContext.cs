using Microsoft.EntityFrameworkCore;
using boat_share.Models;

namespace boat_share.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Boat> Boats { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).ValueGeneratedOnAdd();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt);

                // User-Boat relationship
                entity.HasOne(u => u.Boat)
                    .WithMany()
                    .HasForeignKey(u => u.BoatId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Boat entity configuration
            modelBuilder.Entity<Boat>(entity =>
            {
                entity.HasKey(e => e.BoatId);
                entity.Property(e => e.BoatId).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Type).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.Capacity).IsRequired();
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(10,2)");
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
            });

            // Reservation entity configuration
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.ReservationId);
                entity.Property(e => e.ReservationId).ValueGeneratedOnAdd();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.BoatId).IsRequired();
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.EndTime).IsRequired();
                entity.Property(e => e.TotalCost).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ReservationType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();

                // Foreign key relationships - explicitly configure
                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Boat)
                    .WithMany()
                    .HasForeignKey(r => r.BoatId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.BoatId, e.StartTime, e.EndTime });
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed sample boats first
            modelBuilder.Entity<Boat>().HasData(
                new Boat
                {
                    BoatId = 1,
                    Name = "Ocean Explorer",
                    Type = "Sailing Yacht",
                    Description = "Beautiful 30ft sailing yacht perfect for day trips",
                    Location = "Marina Bay",
                    Capacity = 6,
                    HourlyRate = 150.00m,
                    IsActive = true,
                    AssignedUsersCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Boat
                {
                    BoatId = 2,
                    Name = "Speed Demon",
                    Type = "Motor Boat",
                    Description = "Fast motor boat for thrilling water adventures",
                    Location = "Harbor Point",
                    Capacity = 4,
                    HourlyRate = 200.00m,
                    IsActive = true,
                    AssignedUsersCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Seed a default admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Email = "admin@boatshare.com",
                    Name = "Admin User",
                    Role = "Admin",
                    StandardQuota = 10,
                    SubstitutionQuota = 5,
                    ContingencyQuota = 3,
                    BoatId = 1,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
