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
    }
}