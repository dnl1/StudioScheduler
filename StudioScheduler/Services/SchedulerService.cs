using Microsoft.EntityFrameworkCore;
using StudioScheduler.Data;
using StudioScheduler.Dtos;
using StudioScheduler.Extensions;
using StudioScheduler.Interfaces;
using StudioScheduler.Mappers;
using StudioScheduler.Models;

namespace StudioScheduler.Services
{
    public class SchedulerService : ISchedulerService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppConfig _appConfig;

        public SchedulerService(ApplicationDbContext context, IAppConfig appConfig)
        {
            _context = context;
            _appConfig = appConfig;
        }

        public async Task<(bool IsSuccess, string Message)> Delete(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
                return (false, "Agenda não encontrada");

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }

        public async Task<SchedulerDto> GetScheduleById(int id)
        {
            var schedule = await _context.Schedules
                .Include(s => s.Room)
                .ThenInclude(r => r.Studio)
                .FirstOrDefaultAsync(s => s.ScheduleID == id);

            return schedule.ToDto();
        }

        public async Task<IList<SchedulerDto>> GetSchedules()
        {
            return await _context.Schedules
               .Include(s => s.Room)
               .ThenInclude(r => r.Studio)
               .OrderByDescending(s => s.StartDate)
               .Select(s => s.ToDto())
               .ToListAsync();
        }

        public async Task<IList<AvailableSlots>> GetScheduleSlots(DateTime weekStart)
        {
            weekStart = DateTime.SpecifyKind(weekStart, DateTimeKind.Utc);

            // Calculate the week range
            var (startOfWeek, endOfWeek) = weekStart.GetWeekRange();

            // Query the database for reservations within the week range
            var reservations = await _context.Schedules
                .Where(s => s.StartDate >= startOfWeek && s.EndDate <= endOfWeek)
                .ToListAsync();

            // Create a list to hold the results
            var results = new List<AvailableSlots>();

            // Generate slots for each day in the week
            for (var date = startOfWeek; date <= endOfWeek; date = date.AddDays(1))
            {
                var dailySlots = date.GenerateHourlySlots(_appConfig.FirstHour, _appConfig.LastHour);

                var slotsWithStatus = dailySlots.Select(slot => new Slot
                {
                    Time = slot.ToString("HH:mm"),
                    Status = GetSlotStatus(slot, reservations)
                });

                results.Add(new AvailableSlots
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Slots = slotsWithStatus.ToList()
                });
            }

            return results;
        }

        private string GetSlotStatus(DateTime slot, List<Scheduler> reservations)
        {
            foreach (var reservation in reservations)
            {
                if (slot >= reservation.StartDate && slot < reservation.EndDate)
                {
                    return "unavailable";
                }
            }

            return "available";
        }

        public async Task<(bool IsSuccess, string Message)> Update(int id, Scheduler schedule)
        {
            // Check for overlapping schedules, excluding the current schedule
            var hasOverlap = await _context.Schedules
                .AnyAsync(s =>
                    s.RoomID == schedule.RoomID &&
                    s.ScheduleID != id &&
                    s.StartDate < schedule.EndDate &&
                    s.EndDate > schedule.StartDate);

            if (hasOverlap)
                return (false, "Horário não disponivel");

            _context.Entry(schedule).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return (true, string.Empty);
        }

        public async Task<(bool IsSuccess, string Message)> Create(Scheduler schedule)
        {
            // Check for overlapping schedules
            var hasOverlap = await _context.Schedules
                .AnyAsync(s =>
                    s.RoomID == schedule.RoomID &&
                    s.StartDate < schedule.EndDate &&
                    s.EndDate > schedule.StartDate);

            if (hasOverlap)
                return (false, "Horário não disponivel");

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }
    }
}