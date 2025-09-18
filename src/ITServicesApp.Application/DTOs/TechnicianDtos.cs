using System;

namespace ITServicesApp.Application.DTOs
{
    public class TechnicianDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ServiceCategoryId { get; set; }
        public bool IsActive { get; set; }
        public string? Bio { get; set; }
        public decimal? HourlyRate { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class TechnicianProfileDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = default!;
        public string UserEmail { get; set; } = default!;
        public int ServiceCategoryId { get; set; }
        public string ServiceCategoryName { get; set; } = default!;
        public string? Bio { get; set; }
        public decimal? HourlyRate { get; set; }
        public bool IsActive { get; set; }
        public double AverageRating { get; set; } // Fill via service/repo
        public int ReviewsCount { get; set; }     // Fill via service/repo
    }

    public class CreateTechnicianDto
    {
        public int UserId { get; set; }
        public int ServiceCategoryId { get; set; }
        public string? Bio { get; set; }
        public decimal? HourlyRate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateTechnicianProfileDto
    {
        public int ServiceCategoryId { get; set; }
        public string? Bio { get; set; }
        public decimal? HourlyRate { get; set; }
        public bool IsActive { get; set; }
    }
}
