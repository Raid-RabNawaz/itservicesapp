using ITServicesApp.Domain.Base;

namespace ITServicesApp.Domain.Entities
{
    public class ServiceIssue : AuditableEntity
    {
        public int Id { get; set; }
        public int ServiceCategoryId { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int? EstimatedDurationMinutes { get; set; }

        public ServiceCategory ServiceCategory { get; set; } = default!;
    }
}
