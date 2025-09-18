using MediatR;

namespace ITServicesApp.Domain.Events
{
    public sealed record BookingCancelledDomainEvent(int BookingId, int UserId, int TechnicianId, DateTime ScheduledAtUtc) : INotification;
}
