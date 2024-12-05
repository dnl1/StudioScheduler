using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudioScheduler.Data;
using StudioScheduler.Models;

namespace StudioScheduler.Validators
{
    public class SchedulerValidator : AbstractValidator<Scheduler>
    {
        private readonly ApplicationDbContext _context;

        public SchedulerValidator(ApplicationDbContext context)
        {
            _context = context;

            // Validate BandName
            RuleFor(s => s.BandName)
                .NotEmpty().WithMessage("BandName is required.")
                .MaximumLength(100).WithMessage("BandName cannot exceed 100 characters.");

            // Validate ContactName
            RuleFor(s => s.ContactName)
                .NotEmpty().WithMessage("ContactName is required.")
                .MaximumLength(50).WithMessage("ContactName cannot exceed 50 characters.");

            // Validate MobileNumber
            RuleFor(s => s.MobileNumber)
                .NotEmpty().WithMessage("MobileNumber is required.")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("MobileNumber must be a valid international phone number.");

            // Validate StartDate and EndDate
            RuleFor(s => s.StartDate)
                .LessThan(s => s.EndDate).WithMessage("StartDate must be earlier than EndDate.");

            // Validate Room booking overlap (custom logic can be injected here)
            RuleFor(s => s)
                .MustAsync(async (schedule, cancellation) => await NoOverlappingSchedules(schedule))
                .WithMessage("The room is already booked during the specified time.");
        }

        private async Task<bool> NoOverlappingSchedules(Scheduler schedule)
        {
            // Query the database for overlapping schedules
            var hasOverlap = await _context.Schedules.AnyAsync(existingSchedule =>
                existingSchedule.RoomID == schedule.RoomID && // Same room
                existingSchedule.ScheduleID != schedule.ScheduleID && // Exclude current schedule (for updates)
                existingSchedule.StartDate < schedule.EndDate && // Overlap condition
                existingSchedule.EndDate > schedule.StartDate);  // Overlap condition

            return !hasOverlap; // Return true if no overlap, false if overlap exists
        }
    }
}
