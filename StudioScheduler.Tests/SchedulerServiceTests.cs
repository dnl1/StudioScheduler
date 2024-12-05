using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using StudioScheduler.Data;
using StudioScheduler.Interfaces;
using StudioScheduler.Models;
using StudioScheduler.Services;
using Xunit;

namespace StudioScheduler.Tests
{
    public class SchedulerServiceTests : IDisposable
    {
        private readonly Mock<IAppConfig> _mockConfiguration;
        private readonly ApplicationDbContext _context;

        public SchedulerServiceTests()
        {
            _mockConfiguration = new Mock<IAppConfig>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique DB per test
                .Options;

            _context = new ApplicationDbContext(options);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted(); // Cleanup database after each test
            _context.Dispose();
        }

        [Fact]
        public async Task GetSchedules_ShouldReturnAllSchedulesOrderedByStartDate()
        {
            // Arrange
            var studio = new Studio
            {
                StudioID = 1,
                Name = "Studio One",
                Address = "123 Main St",
                ContactNumber = "119999999"
            };

            var room = new Room
            {
                RoomID = 1,
                RoomName = "Room 1",
                StudioID = studio.StudioID,
                Studio = studio
            };

            _context.Studios.Add(studio);
            _context.Rooms.Add(room);

            _context.Schedules.AddRange(
                new Scheduler
                {
                    ScheduleID = 1,
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(1).AddHours(1),
                    RoomID = room.RoomID,
                    Room = room,
                    BandName = "Band A",
                    ContactName = "Jane Doe",
                    MobileNumber = "1234567890"
                },
                new Scheduler
                {
                    ScheduleID = 2,
                    StartDate = DateTime.UtcNow.AddDays(2),
                    EndDate = DateTime.UtcNow.AddDays(2).AddHours(1),
                    RoomID = room.RoomID,
                    Room = room,
                    BandName = "Band B",
                    ContactName = "John Smith",
                    MobileNumber = "0987654321"
                }
            );
            await _context.SaveChangesAsync();

            var service = new SchedulerService(_context, _mockConfiguration.Object);

            // Act
            var result = await service.GetSchedules();

            // Assert
            result.Should().NotBeNullOrEmpty()
                  .And.HaveCount(2)
                  .And.BeInDescendingOrder(s => s.StartDate);
        }

        [Fact]
        public async Task GetScheduleSlots_ShouldReturnSlotsWithCorrectStatus()
        {
            // Arrange
            var weekStart = new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc);

            // Arrange
            var studio = new Studio
            {
                StudioID = 1,
                Name = "Studio One",
                Address = "123 Main St",
                ContactNumber = "119999999"
            };

            var room = new Room
            {
                RoomID = 1,
                RoomName = "Room 1",
                StudioID = studio.StudioID,
                Studio = studio
            };

            _context.Studios.Add(studio);
            _context.Rooms.Add(room);

            var tomorrow = DateTime.UtcNow.AddDays(1);

            _context.Schedules.AddRange(
                new Scheduler
                {
                    ScheduleID = 1,
                    StartDate = tomorrow.Date.AddHours(9), // 9:00 AM
                    EndDate = tomorrow.AddDays(1).Date.AddHours(10), // 10:00 AM
                    RoomID = room.RoomID,
                    Room = room,
                    BandName = "Band A",
                    ContactName = "Jane Doe",
                    MobileNumber = "1234567890"
                }
            );
            await _context.SaveChangesAsync();

            _mockConfiguration.Setup(c => c.FirstHour).Returns(8);
            _mockConfiguration.Setup(c => c.LastHour).Returns(23);

            var service = new SchedulerService(_context, _mockConfiguration.Object);

            // Act
            var result = await service.GetScheduleSlots(weekStart);

            // Assert
            result.Should().NotBeNullOrEmpty()
                  .And.HaveCount(7);

            result.Should().Contain(day => day.Slots.Any(slot => slot.Time == "09:00" && slot.Status == "unavailable" || slot.Status == "available"));

        }

        [Fact]
        public async Task Create_ShouldAddSchedule_WhenNoConflictExists()
        {
            // Arrange
            var newSchedule = new Scheduler
            {
                ScheduleID = 1,
                StartDate = DateTime.UtcNow.AddHours(9),
                EndDate = DateTime.UtcNow.AddHours(10),
                RoomID = 1,
                BandName = "Test Band",
                ContactName = "John Doe",
                MobileNumber = "1234567890"
            };

            var service = new SchedulerService(_context, _mockConfiguration.Object);

            // Act
            var (isSuccess, message) = await service.Create(newSchedule);

            // Assert
            isSuccess.Should().BeTrue();
            message.Should().BeEmpty();
            _context.Schedules.Should().ContainEquivalentOf(newSchedule);
        }

        [Fact]
        public async Task Create_ShouldFail_WhenOverlapExists()
        {
            // Arrange
            _context.Schedules.Add(new Scheduler
            {
                ScheduleID = 1,
                StartDate = DateTime.UtcNow.AddHours(9),
                EndDate = DateTime.UtcNow.AddHours(10),
                RoomID = 1,
                BandName = "Band A",
                ContactName = "John Doe",
                MobileNumber = "1234567890"
            });
            await _context.SaveChangesAsync();

            var overlappingSchedule = new Scheduler
            {
                ScheduleID = 2,
                StartDate = DateTime.UtcNow.AddHours(9),
                EndDate = DateTime.UtcNow.AddHours(11),
                RoomID = 1,
                BandName = "Band B",
                ContactName = "Jane Smith",
                MobileNumber = "0987654321"
            };

            var service = new SchedulerService(_context, _mockConfiguration.Object);

            // Act
            var (isSuccess, message) = await service.Create(overlappingSchedule);

            // Assert
            isSuccess.Should().BeFalse();
            message.Should().Be("Horário não disponivel");
        }

        [Fact]
        public async Task Delete_ShouldRemoveSchedule_WhenExists()
        {
            // Arrange
            var schedule = new Scheduler
            {
                ScheduleID = 1,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(1),
                RoomID = 1,
                BandName = "Band A",
                ContactName = "John Doe",
                MobileNumber = "1234567890"
            };
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            var service = new SchedulerService(_context, _mockConfiguration.Object);

            // Act
            var (isSuccess, message) = await service.Delete(1);

            // Assert
            isSuccess.Should().BeTrue();
            message.Should().BeEmpty();
            _context.Schedules.Should().BeEmpty();
        }
    }
}