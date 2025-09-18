using System.Collections.Generic;
using ITServicesApp.Domain.Base;

namespace ITServicesApp.Domain.Entities
{
    public class ServiceCategory : AuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }

        public ICollection<ServiceIssue> ServiceIssues { get; set; } = new List<ServiceIssue>();
        public ICollection<Technician> Technicians { get; set; } = new List<Technician>();
    }
}
