using ITServicesApp.Persistence.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace ITServicesApp.Persistence.Interceptors
{
    public sealed class AuditTrailSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var ctx = eventData.Context; if (ctx is null) return base.SavingChanges(eventData, result);

            var logs = new List<AuditLog>();
            foreach (var e in ctx.ChangeTracker.Entries().Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                logs.Add(new AuditLog
                {
                    EntityName = e.Metadata.ClrType.Name,
                    EntityId = TryKey(e),
                    Action = e.State.ToString(),
                    ChangedAtUtc = DateTime.UtcNow,
                    ChangesJson = Serialize(e)
                });
            }
            if (logs.Count > 0) ctx.Set<AuditLog>().AddRange(logs);
            return base.SavingChanges(eventData, result);
        }

        private static string Serialize(EntityEntry e)
        {
            var dict = new Dictionary<string, object?>();
            foreach (var p in e.Properties)
                dict[p.Metadata.Name] = new { Original = p.OriginalValue, Current = p.CurrentValue, p.IsModified };
            return JsonSerializer.Serialize(dict);
        }
        private static string? TryKey(EntityEntry e)
        {
            var key = e.Metadata.FindPrimaryKey(); if (key is null) return null;
            return string.Join("|", key.Properties.Select(p => e.Property(p.Name).CurrentValue?.ToString()));
        }
    }
}
