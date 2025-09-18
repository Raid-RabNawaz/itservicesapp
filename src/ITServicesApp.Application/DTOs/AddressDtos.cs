using System;

namespace ITServicesApp.Application.DTOs
{
    public sealed class AddressDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Label { get; set; } = "Home"; // Home, Office, etc.
        public string Line1 { get; set; } = default!;
        public string? Line2 { get; set; }
        public string City { get; set; } = default!;
        public string State { get; set; } = default!;
        public string PostalCode { get; set; } = default!;
        public string Country { get; set; } = "US";
        public bool IsDefault { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class CreateAddressDto
    {
        public string Label { get; set; } = "Home";
        public string Line1 { get; set; } = default!;
        public string? Line2 { get; set; }
        public string City { get; set; } = default!;
        public string State { get; set; } = default!;
        public string PostalCode { get; set; } = default!;
        public string Country { get; set; } = "US";
        public bool IsDefault { get; set; }
    }

    public sealed class UpdateAddressDto : CreateAddressDto { }
}