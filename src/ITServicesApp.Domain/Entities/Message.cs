namespace ITServicesApp.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }
        public int SenderUserId { get; set; }
        public string Body { get; set; } = default!;
        public DateTime SentAtUtc { get; set; }
        public bool IsRead { get; set; }
        public MessageThread Thread { get; set; } = default!;
    }
}