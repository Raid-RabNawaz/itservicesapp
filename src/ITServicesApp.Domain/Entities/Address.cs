namespace ITServicesApp.Domain.Entities
{
    public class Address
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Label { get; set; } = "Home";
        public string Line1 { get; set; } = default!;
        public string? Line2 { get; set; }
        public string City { get; set; } = default!;
        public string State { get; set; } = default!;
        public string PostalCode { get; set; } = default!;
        public string Country { get; set; } = "US";
        public bool IsDefault { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}