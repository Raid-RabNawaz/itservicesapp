using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface IMessageService
    {
        Task<MessageThreadDto> GetOrCreateThreadForBookingAsync(int bookingId, CancellationToken ct);
        Task<IReadOnlyList<MessageDto>> ListAsync(int threadId, int take, int skip, CancellationToken ct);
        Task<MessageDto> SendAsync(SendMessageDto dto, CancellationToken ct);
        Task MarkThreadReadAsync(int threadId, CancellationToken ct);
    }
}
