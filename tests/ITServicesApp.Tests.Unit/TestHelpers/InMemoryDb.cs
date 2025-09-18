using System;
using Microsoft.EntityFrameworkCore;

namespace ITServicesApp.Tests.Unit.TestHelpers
{
    public static class InMemoryDb
    {
        public static DbContextOptions<ITServicesApp.Persistence.ApplicationDbContext> CreateOptions(string? name = null)
        {
            return new DbContextOptionsBuilder<ITServicesApp.Persistence.ApplicationDbContext>()
                .UseInMemoryDatabase(name ?? $"itservices-{Guid.NewGuid():N}")
                .EnableSensitiveDataLogging()
                .Options;
        }
    }
}
