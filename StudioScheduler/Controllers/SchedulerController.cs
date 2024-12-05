using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudioScheduler.Data;
using StudioScheduler.Dtos;
using StudioScheduler.Extensions;
using StudioScheduler.Interfaces;
using StudioScheduler.Models;

namespace StudioScheduler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulerController : ControllerBase
    {
        private readonly ISchedulerService _schedulerService;

        public SchedulerController(ISchedulerService schedulerService)
        {
            _schedulerService = schedulerService;
        }

        // GET: api/Scheduler
        [HttpGet]
        public async Task<IActionResult> GetSchedules()
        {
            var schedules = await _schedulerService.GetSchedules();
            return Ok(schedules);
        }

        [HttpGet("slots")]
        public async Task<IActionResult> GetScheduleSlots([FromQuery] DateTime weekStart)
        {
            var slots = await _schedulerService.GetScheduleSlots(weekStart);
            return Ok(slots);
        }

        // GET: api/Scheduler/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            var schedule = await _schedulerService.GetScheduleById(id);

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

            var response = await _schedulerService.Create(schedule);

            if (response.IsSuccess)
            {
                return CreatedAtAction(nameof(GetScheduleById), new { id = schedule.ScheduleID }, schedule);
            }

            return UnprocessableEntity(response.Message);
        }

        // PUT: api/Scheduler/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] Scheduler schedule)
        {
            if (id != schedule.ScheduleID)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _schedulerService.Update(id, schedule);

            if (response.IsSuccess)
            {
                return Ok();
            }

            return UnprocessableEntity(response.Message);
        }

        // DELETE: api/Scheduler/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var response = await _schedulerService.Delete(id);

            if (response.IsSuccess)
            {
                return Ok();
            }

            return UnprocessableEntity(response.Message);
        }
    }
}