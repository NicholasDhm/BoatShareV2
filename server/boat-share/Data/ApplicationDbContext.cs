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

                // User-Boat relationship with navigation
                entity.HasOne(u => u.Boat)
                    .WithMany(b => b.Users)
                    .HasForeignKey(u => u.BoatId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.BoatId);
                entity.HasIndex(e => e.Role);
                entity.HasIndex(e => e.DeletedAt);

                // Global query filter for soft delete
                entity.HasQueryFilter(u => u.DeletedAt == null);
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
                entity.Property(e => e.UpdatedAt);
                entity.Property(e => e.DeletedAt);

                // Indexes
                entity.HasIndex(e => e.DeletedAt);

                // Global query filter for soft delete
                entity.HasQueryFilter(b => b.DeletedAt == null);
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

                // Foreign key relationships with navigation properties
                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reservations)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Boat)
                    .WithMany(b => b.Reservations)
                    .HasForeignKey(r => r.BoatId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.BoatId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ReservationType);
                entity.HasIndex(e => e.StartTime);
                entity.HasIndex(e => new { e.BoatId, e.StartTime, e.EndTime });
                entity.HasIndex(e => new { e.BoatId, e.Status, e.StartTime, e.EndTime });
                entity.HasIndex(e => new { e.UserId, e.EndTime });
                entity.HasIndex(e => new { e.UserId, e.ReservationType });

                // Global query filter to match Boat's soft delete filter
                entity.HasQueryFilter(r => r.Boat != null && r.Boat.DeletedAt == null && r.User != null && r.User.DeletedAt == null);
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Use static datetime for seeding to avoid model changes warning
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
                    CreatedAt = seedDate
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
                    CreatedAt = seedDate
                }
            );

            // Seed a default admin user
            // Pre-hashed password for "admin123" (BCrypt hash is static to avoid model changes)
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
                    PasswordHash = "$2a$11$XQUcMJK7fX6xYynk46Luyu9GZPbehLd9tFaaBii7mATFTkTXHlfNu",
                    IsActive = true,
                    CreatedAt = seedDate
                }
            );
        }
    }
}
