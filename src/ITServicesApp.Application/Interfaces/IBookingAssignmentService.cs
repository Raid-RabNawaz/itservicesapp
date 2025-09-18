using System;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface IBookingAssignmentService
    {
        Task<BookingAssignmentResultDto?> FindBestAsync(int serviceCategoryId, int serviceIssueId, DateTime startUtc, int durationMinutes, CancellationToken ct = default);
        Task<bool> IsTechnicianAvailableAsync(int technicianId, DateTime startUtc, int durationMinutes, CancellationToken ct = default);
    }
}
