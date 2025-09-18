namespace ITServicesApp.Domain.Entities
{
    public class MessageThread
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public int TechnicianId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}