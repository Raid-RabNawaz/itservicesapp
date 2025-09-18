using System;
using System.Collections.Generic;
using ITServicesApp.Domain.Enums;

namespace ITServicesApp.Application.DTOs
{
    public class BookingPipelineStartRequestDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int ServiceCategoryId { get; set; }
        public int ServiceIssueId { get; set; }
        public PaymentMethod PreferredPaymentMethod { get; set; } = PaymentMethod.Cash;
        public List<CreateBookingItemDto> Items { get; set; } = new();
        public string? Notes { get; set; }
    }

    public class BookingPipelineAddressDto
    {
        public string Line1 { get; set; } = string.Empty;
        public string? Line2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class BookingPipelineSlotRequestDto
    {
        public DateTime StartUtc { get; set; }
        public DateTime? EndUtc { get; set; }
        public int? DurationMinutes { get; set; }
        public int? TechnicianId { get; set; }
    }

    public class BookingPipelineConfirmDto
    {
        public string? ClientRequestId { get; set; }
        public PaymentMethod? PreferredPaymentMethod { get; set; }
    }

    public class BookingPipelineServiceSelectionDto
    {
        public int? ServiceCategoryId { get; set; }
        public int? ServiceIssueId { get; set; }
        public IReadOnlyList<BookingPipelineItemDto> Items { get; set; } = Array.Empty<BookingPipelineItemDto>();
    }

    public class BookingPipelineItemDto
    {
        public int ServiceIssueId { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Notes { get; set; }
    }

    public class BookingPipelineSlotDto
    {
        public int TechnicianId { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public int? DurationMinutes { get; set; }
        public int? SlotId { get; set; }
    }

    public class BookingPipelineStateDto
    {
        public Guid Id { get; set; }
        public BookingDraftStatus Status { get; set; }
        public bool IsAuthenticatedUser { get; set; }
        public int? UserId { get; set; }
        public string? GuestFullName { get; set; }
        public string? GuestEmail { get; set; }
        public string? GuestPhone { get; set; }
        public BookingPipelineServiceSelectionDto? Service { get; set; }
        public BookingPipelineAddressDto? Address { get; set; }
        public BookingPipelineSlotDto? Slot { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public PaymentMethod PreferredPaymentMethod { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
    }

    public class BookingAssignmentResultDto
    {
        public int TechnicianId { get; set; }
        public int? SlotId { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public int DurationMinutes { get; set; }
    }

    public class BookingPipelineSubmissionResultDto
    {
        public Guid DraftId { get; set; }
        public bool RequiresLogin { get; set; }
        public int? ExistingUserId { get; set; }
        public BookingResponseDto? Booking { get; set; }
    }
}
