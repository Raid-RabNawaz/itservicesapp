using System;
using ITServicesApp.Application.Abstractions;

namespace ITServicesApp.Tests.Unit.TestHelpers
{
    public sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow { get; set; } = DateTime.UtcNow;
    }
}
