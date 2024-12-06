using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudioScheduler.Data;
using StudioScheduler.Models;
using StudioScheduler.Requests;

namespace StudioScheduler.Validators
{
    public class SchedulerValidator : AbstractValidator<ScheduleRequest>
    {
        private readonly ApplicationDbContext _context;

        public SchedulerValidator(ApplicationDbContext context)
        {
            _context = context;

            // Validate BandName
            RuleFor(s => s.BandName)
                .NotEmpty().WithMessage("O nome da banda é obrigatório.")
                .MaximumLength(100).WithMessage("O nome da banda não pode exceder 100 caracteres.");

            // Valida o nome do contato
            RuleFor(s => s.ContactName)
                .NotEmpty().WithMessage("O nome do contato é obrigatório.")
                .MaximumLength(50).WithMessage("O nome do contato não pode exceder 50 caracteres.");

            // Valida o número de telefone
            RuleFor(s => s.MobileNumber)
                .NotEmpty().WithMessage("O número de telefone é obrigatório.")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("O número de telefone deve ser um número internacional válido.");

            // Valida a StartDate e a EndDate
            RuleFor(s => s.StartDate)
                .LessThanOrEqualTo(s => s.EndDate).WithMessage("A data de início deve ser anterior à data de término.");

            // Valida sobreposição de agendamentos (lógica customizada pode ser injetada aqui)
            RuleFor(s => s)
                .MustAsync(async (schedule, cancellation) => await NoOverlappingSchedules(schedule))
                .WithMessage("A sala já está reservada para o horário especificado.");
        }

        private async Task<bool> NoOverlappingSchedules(ScheduleRequest schedule)
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
