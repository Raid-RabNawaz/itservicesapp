using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Infrastructure.Services
{
    public sealed class BookingAssignmentService : IBookingAssignmentService
    {
        private readonly IUnitOfWork _uow;

        public BookingAssignmentService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<BookingAssignmentResultDto?> FindBestAsync(int serviceCategoryId, int serviceIssueId, DateTime startUtc, int durationMinutes, CancellationToken ct = default)
        {
            if (durationMinutes <= 0)
            {
                durationMinutes = 60;
            }

            var requestStart = startUtc;
            var requestEnd = requestStart.AddMinutes(durationMinutes);
            var day = requestStart.Date;
            var dayEnd = day.AddDays(1);

            var techIds = await _uow.Technicians.QueryQualifiedTechnicianIdsAsync(serviceCategoryId, serviceIssueId, ct);
            if (techIds.Count == 0) return null;

            BookingAssignmentResultDto? best = null;
            var bestScore = int.MaxValue;

            foreach (var techId in techIds)
            {
                var daySlots = await _uow.TechnicianSlots.GetAvailableAsync(techId, day, ct);
                var matchingSlot = daySlots.FirstOrDefault(s => s.StartUtc <= requestStart && requestEnd <= s.EndUtc);
                if (matchingSlot == null) continue;

                if ((matchingSlot.EndUtc - requestStart).TotalMinutes < durationMinutes) continue;
                if (await _uow.Bookings.HasOverlapAsync(techId, requestStart, requestEnd, ct)) continue;

                var conflicts = await _uow.TechnicianUnavailabilities.ListForTechnicianAsync(techId, requestStart, requestEnd, ct);
                if (conflicts.Any()) continue;

                var workload = (await _uow.Bookings.ListAsync(b =>
                    b.TechnicianId == techId &&
                    b.ScheduledStartUtc >= day &&
                    b.ScheduledStartUtc < dayEnd, ct)).Count;

                if (best == null || workload < bestScore)
                {
                    bestScore = workload;
                    best = new BookingAssignmentResultDto
                    {
                        TechnicianId = techId,
                        SlotId = matchingSlot.Id,
                        StartUtc = requestStart,
                        EndUtc = requestEnd,
                        DurationMinutes = durationMinutes
                    };
                }
            }

            return best;
        }

        public async Task<bool> IsTechnicianAvailableAsync(int technicianId, DateTime startUtc, int durationMinutes, CancellationToken ct = default)
        {
            if (durationMinutes <= 0)
            {
                durationMinutes = 60;
            }

            var requestStart = startUtc;
            var requestEnd = requestStart.AddMinutes(durationMinutes);
            var day = requestStart.Date;

            var slots = await _uow.TechnicianSlots.GetAvailableAsync(technicianId, day, ct);
            var matchingSlot = slots.FirstOrDefault(s => s.StartUtc <= requestStart && requestEnd <= s.EndUtc);
            if (matchingSlot == null) return false;
            if ((matchingSlot.EndUtc - requestStart).TotalMinutes < durationMinutes) return false;

            if (await _uow.Bookings.HasOverlapAsync(technicianId, requestStart, requestEnd, ct)) return false;

            var conflicts = await _uow.TechnicianUnavailabilities.ListForTechnicianAsync(technicianId, requestStart, requestEnd, ct);
            if (conflicts.Any()) return false;

            return true;
        }
    }
}
