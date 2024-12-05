using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudioScheduler.Data;
using StudioScheduler.Dtos;
using StudioScheduler.Models;

namespace StudioScheduler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SchedulerController(ApplicationDbContext context)
        {
            _context = context;
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