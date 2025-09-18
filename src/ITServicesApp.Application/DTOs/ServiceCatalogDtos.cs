namespace ITServicesApp.Application.DTOs
{
    public class ServiceCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
    }

    public class CreateServiceCategoryDto
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
    }

    public class UpdateServiceCategoryDto
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
    }

    public class ServiceIssueDto
    {
        public int Id { get; set; }
        public int ServiceCategoryId { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public decimal BasePrice { get; set; }
    }

    public class CreateServiceIssueDto
    {
        public int ServiceCategoryId { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public decimal BasePrice { get; set; }
    }

    public class UpdateServiceIssueDto
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public decimal BasePrice { get; set; }
    }
}
