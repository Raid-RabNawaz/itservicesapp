using System;
using System.Collections.Generic;
using MediatR;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.UseCases.Bookings.Queries.GetAvailableSlots
{
    /// <summary>
    /// Returns available slots for a given category/issue and day.
    /// </summary>
    public sealed record GetAvailableSlotsQuery(int ServiceCategoryId, int ServiceIssueId, DateTime DayUtc, int? DurationMinutes = null)
        : IRequest<IReadOnlyList<TechnicianSlotDto>>;
}
