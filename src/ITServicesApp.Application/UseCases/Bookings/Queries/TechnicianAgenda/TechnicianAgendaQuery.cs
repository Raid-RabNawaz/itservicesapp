using System;
using System.Collections.Generic;
using ITServicesApp.Application.DTOs;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Queries.TechnicianAgenda
{
    /// <summary>Calendar view for a technician for a day (bookings + busy blocks).</summary>
    public sealed record TechnicianAgendaQuery(int TechnicianId, DateTime DayUtc)
        : IRequest<IReadOnlyList<TechnicianSlotDto>>;
}
