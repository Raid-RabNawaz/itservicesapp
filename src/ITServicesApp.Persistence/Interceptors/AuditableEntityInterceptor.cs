using ITServicesApp.Application.Abstractions;
using ITServicesApp.Domain.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ITServicesApp.Persistence.Interceptors
{
    public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _current;
        private readonly IDateTimeProvider _clock;
        public AuditableEntityInterceptor(ICurrentUserService current, IDateTimeProvider clock) { _current = current; _clock = clock; }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var ctx = eventData.Context; if (ctx is null) return base.SavingChanges(eventData, result);
            foreach (var e in ctx.ChangeTracker.Entries<AuditableEntity>())
            {
                if (e.State == EntityState.Added)
                { e.Entity.CreatedAtUtc = _clock.UtcNow; e.Entity.CreatedBy = _current.UserId.ToString(); }
                if (e.State == EntityState.Modified)
                { e.Entity.ModifiedAtUtc = _clock.UtcNow; e.Entity.ModifiedBy = _current.UserId.ToString(); }
            }
            return base.SavingChanges(eventData, result);
        }
    }
}
