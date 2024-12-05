using Microsoft.EntityFrameworkCore;
using StudioScheduler.Models;

namespace StudioScheduler.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Studio> Studios { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Scheduler> Schedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Scheduler>().ToTable(nameof(Scheduler));
            modelBuilder.Entity<Studio>().ToTable("Studios");
            modelBuilder.Entity<Room>().ToTable("Rooms");

            modelBuilder.Entity<Scheduler>()
                .HasKey(s => s.ScheduleID); // Explicitly set the primary key if necessary

            modelBuilder.Entity<Scheduler>()
                .HasOne(s => s.Room)
                .WithMany(r => r.Schedules)
                .HasForeignKey(s => s.RoomID);

            // Define relationships
            modelBuilder.Entity<Studio>()
                .HasMany(s => s.Rooms)
                .WithOne(r => r.Studio)
                .HasForeignKey(r => r.StudioID);

            modelBuilder.Entity<Room>()
                .HasMany(r => r.Schedules)
                .WithOne(s => s.Room)
                .HasForeignKey(s => s.RoomID);
        }

        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                // Process only entities being added or modified
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    // Loop through all properties
                    foreach (var property in entry.Properties)
                    {
                        if (property.Metadata.ClrType == typeof(DateTime))
                        {
                            // Ensure DateTime is UTC
                            var originalValue = (DateTime)property.CurrentValue!;
                            property.CurrentValue = DateTime.SpecifyKind(originalValue, DateTimeKind.Utc);
                        }
                        else if (property.Metadata.ClrType == typeof(DateTime?))
                        {
                            // Handle nullable DateTime
                            var originalValue = (DateTime?)property.CurrentValue;
                            if (originalValue.HasValue)
                            {
                                property.CurrentValue = DateTime.SpecifyKind(originalValue.Value, DateTimeKind.Utc);
                            }
                        }
                    }
                }
            }

            return base.SaveChanges();
        }
    }
}