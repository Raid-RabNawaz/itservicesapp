using ITServicesApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ITServicesApp.Persistence.Interceptors
{
    public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var ctx = eventData.Context; if (ctx is null) return base.SavingChanges(eventData, result);
            foreach (var e in ctx.ChangeTracker.Entries<ISoftDeletable>())
            {
                if (e.State == EntityState.Deleted)
                {
                    e.State = EntityState.Modified;
                    e.Entity.IsDeleted = true;
                    e.Entity.DeletedAtUtc = DateTime.UtcNow;
                }
            }
            return base.SavingChanges(eventData, result);
        }
    }
}
