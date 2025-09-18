using ITServicesApp.Domain.Entities;
using MediatR;

namespace ITServicesApp.Domain.Events
{
    public sealed record BookingCreatedDomainEvent(int BookingId, int UserId, int TechnicianId, DateTime ScheduledAtUtc) : INotification;
}
