using System;
using ITServicesApp.Domain.Base;

namespace ITServicesApp.Domain.Entities
{
    public class TechnicianSlot : AuditableEntity
    {
        public int Id { get; set; }
        public int TechnicianId { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }

        public Technician Technician { get; set; } = default!;
    }
}
