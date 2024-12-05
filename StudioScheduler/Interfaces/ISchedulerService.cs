using Microsoft.AspNetCore.Mvc;
using StudioScheduler.Dtos;
using StudioScheduler.Models;

namespace StudioScheduler.Interfaces
{
    public interface ISchedulerService
    {
        Task<IList<SchedulerDto>> GetSchedules();

        Task<IList<AvailableSlots>> GetScheduleSlots(DateTime weekStart);

        Task<SchedulerDto> GetScheduleById(int id);

        Task<(bool IsSuccess, string Message)> Create(Scheduler schedule);
        Task<(bool IsSuccess, string Message)> Update(int id, Scheduler schedule);

        Task<(bool IsSuccess, string Message)> Delete(int id);
    }
}