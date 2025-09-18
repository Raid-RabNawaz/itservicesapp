using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using ITServicesApp.Persistence;

namespace ITServicesApp.Infrastructure.Services
{
    public class ServiceReportService : IServiceReportService
    {
        private readonly IServiceReportRepository _repo; private readonly ApplicationDbContext _db;
        public ServiceReportService(IServiceReportRepository repo, ApplicationDbContext db) { _repo = repo; _db = db; }
        public async Task<ServiceReportDto> SubmitAsync(ServiceReportDto dto, CancellationToken ct)
        {
            var entity = await _repo.GetByBookingAsync(dto.BookingId, ct) ?? new ServiceReport { BookingId = dto.BookingId };
            entity.ProblemsDiagnosed = dto.ProblemsDiagnosed; entity.ActionsTaken = dto.ActionsTaken; entity.PartsUsedCsv = dto.PartsUsed != null ? string.Join(',', dto.PartsUsed) : null; entity.TimeSpentMinutes = dto.TimeSpentMinutes; entity.SubmittedAtUtc = DateTime.UtcNow;
            if (entity.Id == 0) await _repo.AddAsync(entity, ct); else _repo.Update(entity);
            await _db.SaveChangesAsync(ct);
            return await GetByBookingAsync(dto.BookingId, ct) ?? dto;
        }
        public async Task<ServiceReportDto?> GetByBookingAsync(int bookingId, CancellationToken ct)
        {
            var e = await _repo.GetByBookingAsync(bookingId, ct);
            if (e == null) return null;
            return new ServiceReportDto { BookingId = e.BookingId, ProblemsDiagnosed = e.ProblemsDiagnosed, ActionsTaken = e.ActionsTaken, PartsUsed = e.PartsUsedCsv?.Split(',').ToList(), TimeSpentMinutes = e.TimeSpentMinutes, SubmittedAtUtc = e.SubmittedAtUtc };
        }
    }
}