using StudioScheduler.Dtos;
using StudioScheduler.Models;
using StudioScheduler.Requests;

namespace StudioScheduler.Mappers
{
    public static class ScheduleMapper
    {
        public static Scheduler ToModel(this ScheduleRequest request) => 
            new Scheduler
            {
                ScheduleID = request.ScheduleID,
                BandName = request.BandName,
                ContactName = request.ContactName,
                MobileNumber = request.MobileNumber,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RoomID = request.RoomID,
            };

        public static SchedulerDto ToDto(this Scheduler? s) =>
            new SchedulerDto
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
            };
    }
}