using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Persistence;
using ITServicesApp.Tests.Unit.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class SoftDeleteFilterTests
{
    [Fact]
    public async Task Soft_deleted_entities_are_filtered_out()
    {
        var opts = InMemoryDb.CreateOptions();

        // seed + soft delete
        await using (var db = new ApplicationDbContext(opts))
        {
            var u = new User { Email = "x@y.com", FullName = "X", PasswordHash = "h" };
            db.Users.Add(u);
            await db.SaveChangesAsync();

            u.IsDeleted = true;
            await db.SaveChangesAsync();

            // sanity: row still in store when bypassing filters
            (await db.Users.IgnoreQueryFilters().AnyAsync(x => x.Id == u.Id)).Should().BeTrue();
        }

        // assert via LINQ in a fresh context (filters apply)
        await using (var db = new ApplicationDbContext(opts))
        {
            var filtered = await db.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == 1); // or capture id above
            filtered.Should().BeNull();
        }
    }

}
