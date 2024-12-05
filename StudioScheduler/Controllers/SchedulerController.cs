using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudioScheduler.Data;
using StudioScheduler.Dtos;
using StudioScheduler.Extensions;
using StudioScheduler.Models;

namespace StudioScheduler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public SchedulerController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Scheduler
        [HttpGet]
        public async Task<IActionResult> GetSchedules()
        {
            var schedules = await _context.Schedules
                   .Include(s => s.Room)
                   .ThenInclude(r => r.Studio)
                   .Select(s => new SchedulerDto
                   {
                       ScheduleID = s.ScheduleID,
                       BandName = s.BandName,
                       ContactName = s.ContactName,
                       MobileNumber = s.MobileNumber,
                       StartDate = s.StartDate,
                       EndDate = s.EndDate,
                       Room = new RoomDto
                       {
                           RoomID = s.Room.RoomID,
                           RoomName = s.Room.RoomName,
                           Studio = new StudioDto
                           {
                               StudioID = s.Room.Studio.StudioID,
                               Name = s.Room.Studio.Name,
                               Address = s.Room.Studio.Address
                           }
                       }
                   })
                   .ToListAsync();

            return Ok(schedules);
        }

        [HttpGet("slots")]
        public async Task<IActionResult> GetScheduleSlots([FromQuery] DateTime weekStart)
        {
            weekStart = DateTime.SpecifyKind(weekStart, DateTimeKind.Utc);

            var firstHour = _configuration.GetValue<int>("FirstHour");
            var lastHour = _configuration.GetValue<int>("LastHour");

            // Calculate the week range
            var (startOfWeek, endOfWeek) = weekStart.GetWeekRange();

            // Query the database for reservations within the week range
            var reservations = await _context.Schedules
                .Where(s => s.StartDate >= startOfWeek && s.EndDate <= endOfWeek)
                .ToListAsync();

            // Create a list to hold the results
            var results = new List<object>();

            // Generate slots for each day in the week
            for (var date = startOfWeek; date <= endOfWeek; date = date.AddDays(1))
            {
                var dailySlots = date.GenerateHourlySlots(firstHour, lastHour);

                var slotsWithStatus = dailySlots.Select(slot => new
                {
                    time = slot.ToString("HH:mm"),
                    status = GetSlotStatus(slot, reservations)
                });

                results.Add(new
                {
                    date = date.ToString("yyyy-MM-dd"),
                    slots = slotsWithStatus
                });
            }

            return Ok(results);
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

        // GET: api/Scheduler/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            var schedule = await _context.Schedules
                .Include(s => s.Room)
                .ThenInclude(r => r.Studio)
                .FirstOrDefaultAsync(s => s.ScheduleID == id);

            if (schedule == null)
                return NotFound();

            return Ok(schedule);
        }

        // POST: api/Scheduler
        // POST: api/Scheduler
        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] Scheduler schedule)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check for overlapping schedules
            var hasOverlap = await _context.Schedules
                .AnyAsync(s =>
                    s.RoomID == schedule.RoomID &&
                    s.StartDate < schedule.EndDate &&
                    s.EndDate > schedule.StartDate);

            if (hasOverlap)
                return Conflict(new { Message = "The room is already booked during the specified time." });

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetScheduleById), new { id = schedule.ScheduleID }, schedule);
        }

        // PUT: api/Scheduler/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] Scheduler schedule)
        {
            if (id != schedule.ScheduleID)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check for overlapping schedules, excluding the current schedule
            var hasOverlap = await _context.Schedules
                .AnyAsync(s =>
                    s.RoomID == schedule.RoomID &&
                    s.ScheduleID != id &&
                    s.StartDate < schedule.EndDate &&
                    s.EndDate > schedule.StartDate);

            if (hasOverlap)
                return Conflict(new { Message = "The room is already booked during the specified time." });

            _context.Entry(schedule).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduleExists(id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        // DELETE: api/Scheduler/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
                return NotFound();

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ScheduleExists(int id)
        {
            return _context.Schedules.Any(e => e.ScheduleID == id);
        }
    }
}