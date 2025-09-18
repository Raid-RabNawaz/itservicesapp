using System;
using System.Collections.Generic;

namespace ITServicesApp.Application.DTOs
{
    public sealed class MessageThreadDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public int TechnicianId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int UnreadForCustomer { get; set; }
        public int UnreadForTechnician { get; set; }
    }

    public sealed class MessageDto
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }
        public int SenderUserId { get; set; }
        public string Body { get; set; } = default!;
        public DateTime SentAtUtc { get; set; }
        public bool IsRead { get; set; }
    }

    public sealed class SendMessageDto
    {
        public int ThreadId { get; set; }
        public string Body { get; set; } = default!;
    }
}
