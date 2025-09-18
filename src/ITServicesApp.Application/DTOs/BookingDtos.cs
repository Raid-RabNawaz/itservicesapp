using System;
using System.Collections.Generic;
using ITServicesApp.Domain.Enums;

namespace ITServicesApp.Application.DTOs
{
    public class BookingDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TechnicianId { get; set; }

        // Legacy single-service fields (will be phased out once clients fully adopt multi-item flow)
        public int ServiceCategoryId { get; set; }
        public int ServiceIssueId { get; set; }

        public DateTime ScheduledStartUtc { get; set; }
        public DateTime ScheduledEndUtc { get; set; }
        public BookingStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public string CustomerFullName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public BookingAddressDto Address { get; set; } = new();

        public PaymentMethod PreferredPaymentMethod { get; set; }
        public decimal EstimatedTotal { get; set; }
        public decimal? FinalTotal { get; set; }

        public IReadOnlyList<BookingItemDto> Items { get; set; } = Array.Empty<BookingItemDto>();
    }

    // Compact response that includes technician/user display + payment summary
    public class BookingResponseDto : BookingDto
    {
        public string? TechnicianName { get; set; }
        public string? UserFullName { get; set; }
        public PaymentSummaryDto? Payment { get; set; }
    }

    public class BookingItemDto
    {
        public int Id { get; set; }
        public int ServiceIssueId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? ServiceDescription { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
        public string? Notes { get; set; }
    }

    public class PaymentSummaryDto
    {
        public string Status { get; set; } = "Pending";
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? ProviderPaymentId { get; set; }
    }

    public class GuestBookingRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public BookingAddressDto Address { get; set; } = new();
        public int TechnicianId { get; set; }
        public int? ServiceCategoryId { get; set; }
        public int? ServiceIssueId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public PaymentMethod PreferredPaymentMethod { get; set; } = PaymentMethod.Cash;
        public string? Notes { get; set; }
        public List<CreateBookingItemDto> Items { get; set; } = new();
        public string? ClientRequestId { get; set; }
    }

    public class GuestBookingResponseDto
    {
        public bool RequiresLogin { get; set; }
        public int? ExistingUserId { get; set; }
        public BookingResponseDto? Booking { get; set; }
    }

    public class BookingAddressDto
    {
        public int? AddressId { get; set; }
        public string? Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
    }

    public class CreateBookingItemDto
    {
        public int ServiceIssueId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal? UnitPrice { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateBookingDto
    {
        public int UserId { get; set; }
        public int TechnicianId { get; set; }

        // Legacy single-service fields (optional once items are used exclusively)
        public int? ServiceCategoryId { get; set; }
        public int? ServiceIssueId { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public BookingAddressDto? Address { get; set; }
        public string? Notes { get; set; }
        public PaymentMethod PreferredPaymentMethod { get; set; } = PaymentMethod.Cash;

        public string? GuestFullName { get; set; }
        public string? GuestEmail { get; set; }
        public string? GuestPhone { get; set; }

        public List<CreateBookingItemDto> Items { get; set; } = new();

        // OPTIONAL convenience: if you prefer sending via DTO
        public string? ClientRequestId { get; set; }
    }

    public class UpdateBookingNotesDto
    {
        public int BookingId { get; set; }
        public string? Notes { get; set; }
    }

    public class CancelBookingDto
    {
        public int BookingId { get; set; }
    }
}
