using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces
{
    public interface ISettingsService
    {
        Task<PlatformSettingsDto> GetAsync(CancellationToken ct);
        Task UpdateAsync(UpdatePlatformSettingsDto dto, CancellationToken ct);
        Task<IReadOnlyList<NotificationTemplateDto>> ListTemplatesAsync(int take, int skip, CancellationToken ct);
        Task<NotificationTemplateDto> CreateTemplateAsync(NotificationTemplateDto dto, CancellationToken ct);
        Task UpdateTemplateAsync(int id, NotificationTemplateDto dto, CancellationToken ct);
        Task DeleteTemplateAsync(int id, CancellationToken ct);
    }
}
