using AutoMapper;
using ITServicesApp.Application.MappingProfiles;
using Microsoft.Extensions.Logging.Abstractions;

namespace ITServicesApp.Tests.Unit.TestHelpers
{
    public static class AutoMapperTestConfig
    {
        /// <summary>
        /// Creates a mapper for tests. Set validate=true only when you are explicitly testing mapping config.
        /// </summary>
        public static IMapper Create(bool validate = false)
        {
            var expr = new MapperConfigurationExpression();
            expr.AddProfile(new AutoMapperProfile());

            var cfg = new MapperConfiguration(expr, NullLoggerFactory.Instance);

            if (validate)
            {
                // Opt-in validation (VERY strict): only enable in a dedicated mapping test.
                cfg.AssertConfigurationIsValid();
            }

            return cfg.CreateMapper();
        }
    }
}
