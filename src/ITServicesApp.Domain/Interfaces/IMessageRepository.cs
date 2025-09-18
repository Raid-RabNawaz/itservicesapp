using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Domain.Interfaces
{
    public interface IMessageRepository
    {
        Task<MessageThread?> GetThreadByBookingAsync(int bookingId, CancellationToken ct);
        Task<MessageThread> AddThreadAsync(MessageThread thread, CancellationToken ct);
        Task<Message> AddMessageAsync(Message message, CancellationToken ct);
        Task<IReadOnlyList<Message>> ListMessagesAsync(int threadId, int take, int skip, CancellationToken ct);
    }
}