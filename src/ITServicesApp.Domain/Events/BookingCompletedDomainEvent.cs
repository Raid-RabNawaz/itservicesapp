using MediatR;

namespace ITServicesApp.Domain.Events
{
    public sealed record BookingCompletedDomainEvent(int BookingId) : INotification;
}
