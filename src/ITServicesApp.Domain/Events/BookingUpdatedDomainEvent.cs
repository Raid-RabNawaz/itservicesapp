using MediatR;

namespace ITServicesApp.Domain.Events
{
    public sealed record BookingUpdatedDomainEvent(
        int BookingId, int UserId, int TechnicianId, DateTime ScheduledAtUtc, string Reason) : INotification;
}
