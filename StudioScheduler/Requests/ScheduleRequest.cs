using System.ComponentModel.DataAnnotations;

namespace StudioScheduler.Requests
{
    public class ScheduleRequest
    {
        public int ScheduleID { get; set; }

        /// <summary>
        /// The name of the band or entity booking the schedule.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "Band name cannot exceed 100 characters.")]
        public string BandName { get; set; }

        /// <summary>
        /// The name of the primary contact person.
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "Contact name cannot exceed 50 characters.")]
        public string ContactName { get; set; }

        /// <summary>
        /// The mobile number of the contact person.
        /// </summary>
        [Required]
        [Phone(ErrorMessage = "Invalid mobile number format.")]
        public string MobileNumber { get; set; }

        /// <summary>
        /// The ID of the room being booked.
        /// </summary>
        [Required]
        public int RoomID { get; set; }

        /// <summary>
        /// The start date and time of the schedule.
        /// </summary>
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date and time of the schedule.
        /// </summary>
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }
    }
}