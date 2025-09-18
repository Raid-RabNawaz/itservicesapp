using System;
using System.Collections.Generic;

namespace ITServicesApp.Application.DTOs
{
    public sealed class InvoiceDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string Number { get; set; } = default!; // e.g., INV-2025-000123
        public DateTime IssuedAtUtc { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "Paid"; // Paid, Open, Void
    }
}
