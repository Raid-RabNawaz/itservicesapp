using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using ITServicesApp.Persistence;

namespace ITServicesApp.Infrastructure.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _settings; private readonly INotificationTemplateRepository _templates; private readonly ApplicationDbContext _db;
        public SettingsService(ISettingsRepository settings, INotificationTemplateRepository templates, ApplicationDbContext db) { _settings = settings; _templates = templates; _db = db; }
        public async Task<PlatformSettingsDto> GetAsync(CancellationToken ct)
        {
            var s = await _settings.GetSingletonAsync(ct);
            return new PlatformSettingsDto { TechnicianCommissionRate = s.TechnicianCommissionRate, CancellationPolicyHours = s.CancellationPolicyHours, Currency = s.Currency };
        }
        public async Task UpdateAsync(UpdatePlatformSettingsDto dto, CancellationToken ct)
        {
            var s = await _settings.GetSingletonAsync(ct);
            s.TechnicianCommissionRate = dto.TechnicianCommissionRate; s.CancellationPolicyHours = dto.CancellationPolicyHours; s.Currency = dto.Currency; s.ModifiedAtUtc = DateTime.UtcNow;
            _settings.Update(s);
            await _db.SaveChangesAsync(ct);
        }
        public async Task<IReadOnlyList<NotificationTemplateDto>> ListTemplatesAsync(int take, int skip, CancellationToken ct)
        {
            var list = await _templates.ListAsync(null, ct);
            return list.Select(x => new NotificationTemplateDto { Id = x.Id, Key = x.Key, Channel = x.Channel, Subject = x.Subject, Body = x.Body, IsActive = x.IsActive }).ToList();
        }
        public async Task<NotificationTemplateDto> CreateTemplateAsync(NotificationTemplateDto dto, CancellationToken ct)
        {
            var e = new NotificationTemplate { Key = dto.Key, Channel = dto.Channel, Subject = dto.Subject, Body = dto.Body, IsActive = dto.IsActive, CreatedAtUtc = DateTime.UtcNow };
            await _templates.AddAsync(e, ct); await _db.SaveChangesAsync(ct);
            dto.Id = e.Id; return dto;
        }
        public async Task UpdateTemplateAsync(int id, NotificationTemplateDto dto, CancellationToken ct)
        {
            var e = await _templates.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException();
            e.Key = dto.Key; e.Channel = dto.Channel; e.Subject = dto.Subject; e.Body = dto.Body; e.IsActive = dto.IsActive; e.ModifiedAtUtc = DateTime.UtcNow;
            _templates.Update(e); await _db.SaveChangesAsync(ct);
        }
        public async Task DeleteTemplateAsync(int id, CancellationToken ct)
        {
            var e = await _templates.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException();
            _templates.Delete(e); await _db.SaveChangesAsync(ct);
        }
    }
}