using System;

namespace ITServicesApp.Application.DTOs
{
    public class TechnicianReviewDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int TechnicianId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }      // 1..5
        public string? Comment { get; set; }
        public DateTime SubmittedAtUtc { get; set; }
    }

    public class CreateReviewDto
    {
        public int BookingId { get; set; }
        public int TechnicianId { get; set; }
        public int Rating { get; set; }      // 1..5
        public string? Comment { get; set; }
    }
}
