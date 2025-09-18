using System;
using System.Text;
using ITServicesApp.API.Policies;
using ITServicesApp.API.Policies.Handlers;
using ITServicesApp.API.Policies.Requirements;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using ITServicesApp.API.Swagger;
using ITServicesApp.API.Observability.HealthChecks;
using ITServicesApp.API.RateLimiting;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.Behaviors;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.Interfaces.Security;
using ITServicesApp.Application.MappingProfiles;
using ITServicesApp.Infrastructure.Caching;
using ITServicesApp.Infrastructure.Localization;
using ITServicesApp.Infrastructure.Security;
using ITServicesApp.Infrastructure.Services;
using ITServicesApp.Infrastructure.Services.Background;
using ITServicesApp.Persistence;
using ITServicesApp.Persistence.Interceptors;
using ITServicesApp.Persistence.Repositories;
using ITServicesApp.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ITServicesApp.Application.Options;
using ITServicesApp.Application.Interfaces.Notifications;
using ITServicesApp.Infrastructure.Services.Notifications;
using AutoMapper;
using StackExchange.Redis;
using ITServicesApp.API.Middlewares;
using ITServicesApp.Application.DTOs;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Configuration.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config)
        {
            // Options
            services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));
            services.Configure<StripeOptions>(config.GetSection(StripeOptions.SectionName));
            services.Configure<SmtpOptions>(config.GetSection(SmtpOptions.SectionName));
            services.Configure<RedisOptions>(config.GetSection(RedisOptions.SectionName));
            services.Configure<FrontendOptions>(config.GetSection(FrontendOptions.SectionName));

            // EF Interceptors & DbContext
            services.AddScoped<AuditableEntityInterceptor>();
            services.AddScoped<SoftDeleteInterceptor>();
            services.AddScoped<AuditTrailSaveChangesInterceptor>();

            services.AddDbContext<ApplicationDbContext>((sp, opt) =>
            {
                opt.UseSqlServer(config.GetConnectionString("DefaultConnection"));
                opt.AddInterceptors(
                    sp.GetRequiredService<AuditableEntityInterceptor>(),
                    sp.GetRequiredService<SoftDeleteInterceptor>(),
                    sp.GetRequiredService<AuditTrailSaveChangesInterceptor>());
            });

            // Middlewares
            services.AddTransient<ErrorHandlingMiddleware>();
            services.AddTransient<CorrelationIdMiddleware>();
            services.AddTransient<RequestLoggingMiddleware>();

            // Repositories & UoW
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookingDraftRepository, BookingDraftRepository>();
            services.AddScoped<IBookingItemRepository, BookingItemRepository>();
            services.AddScoped<ITechnicianSlotRepository, TechnicianSlotRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITechnicianRepository, TechnicianRepository>();
            services.AddScoped<ITechnicianExpertiseRepository, TechnicianExpertiseRepository>();
            services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
            services.AddScoped<IServiceIssueRepository, ServiceIssueRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<ITechnicianUnavailabilityRepository, TechnicianUnavailabilityRepository>();
            services.AddScoped<ITechnicianReviewRepository, TechnicianReviewRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddHttpContextAccessor(); 
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IServiceReportRepository, ServiceReportRepository>();
            services.AddScoped<ISettingsRepository, SettingsRepository>();
            services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
            services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IMessageService, MessagingService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IEarningsService, EarningsService>();
            services.AddScoped<IServiceReportService, ServiceReportService>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<ISocialAuthService, SocialAuthService>();
            services.AddScoped<IBookingAssignmentService, BookingAssignmentService>();
            services.AddScoped<ITechnicianDashboardService, TechnicianDashboardService>();
            services.AddScoped<ICustomerDashboardService, CustomerDashboardService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddScoped<IEmailService, EmailService>();

            // ADD THESE MISSING SERVICES:   
            services.AddScoped<IPasswordResetService, PasswordResetService>();
            services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<StripeService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<ITechnicianService, TechnicianService>();
            services.AddScoped<ITechnicianSlotService, TechnicianSlotService>();

            services.AddValidatorsFromAssemblyContaining<DashboardStatsDto>(ServiceLifetime.Transient);
            services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(AutoMapperProfile).Assembly);
            });

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditingBehavior<,>));

            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();


            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            // Authentication & Authorization - handled by AuthExtensions

            services.AddSingleton<IAuthorizationHandler, MustBeAdminHandler>();
            services.AddAuthorization(options =>
            {
                RolePolicies.RegisterPolicies(options);
                options.AddPolicy("AdminAccess", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new MustBeAdminRequirement());
                });
            });

            // Redis cache (optional)
            var redisOpts = config.GetSection(RedisOptions.SectionName).Get<RedisOptions>();
            if (redisOpts != null && !string.IsNullOrWhiteSpace(redisOpts.ConnectionString))
            {
                services.AddStackExchangeRedisCache(opt => opt.Configuration = redisOpts.ConnectionString);
                services.AddSingleton<IConnectionMultiplexer>(sp => 
                    ConnectionMultiplexer.Connect(redisOpts.ConnectionString));
                services.AddSingleton<ICacheService, RedisCacheService>();
            }
            else
            {
                services.AddSingleton<ICacheService, NoOpCacheService>();
            }

            // Hangfire
            services.AddHangfire(cfg => cfg
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(config.GetConnectionString("DefaultConnection"),
                    new SqlServerStorageOptions { PrepareSchemaIfNecessary = true }));
            services.AddHangfireServer();

            // Health checks
            services.AddHealthChecks()
                .AddSqlServer(config.GetConnectionString("DefaultConnection")!, tags: new[] { HealthCheckTags.Database });

            // Rate limiting
            services.AddRateLimiter(o => FixedWindowPolicy.Configure(o));

            // CORS for frontend
            services.AddCors(o => o.AddPolicy("frontend", b =>
                b.WithOrigins("http://localhost:5154", "http://localhost:4200", "http://localhost:3000", "https://localhost:3000")
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials()
            ));

            services.AddLocalization();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(SwaggerConfig.Configure);
            services.AddHttpLogging(o => o.LoggingFields = HttpLoggingFields.All);

            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<INotificationChannel, InAppNotificationChannel>();
            services.AddScoped<INotificationChannel, EmailNotificationChannel>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();
            services.AddScoped<BookingReminderJob>();

            ValidateServiceRegistrations(services);

            return services;

        }


        private static void ValidateServiceRegistrations(IServiceCollection services)
        {
            var serviceTypes = services.Select(s => s.ServiceType).ToList();
            var implementationTypes = services.Select(s => s.ImplementationType).Where(t => t != null).ToList();

            // Find controllers and check their dependencies
            var controllerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var controllerType in controllerTypes)
            {
                var constructor = controllerType.GetConstructors().FirstOrDefault();
                if (constructor != null)
                {
                    foreach (var parameter in constructor.GetParameters())
                    {
                        if (!serviceTypes.Contains(parameter.ParameterType))
                        {
                            Console.WriteLine($" MISSING: {parameter.ParameterType.Name} required by {controllerType.Name}");
                        }
                    }
                }
            }
        }
        private static SymmetricSecurityKey CreateSigningKey(JwtOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.Key))
            {
                throw new InvalidOperationException("Jwt:Key configuration value is required.");
            }

            var keyBytes = GetKeyBytes(options.Key);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            if (!string.IsNullOrWhiteSpace(options.KeyId))
            {
                signingKey.KeyId = options.KeyId;
            }

            return signingKey;
        }



        private static byte[] GetKeyBytes(string key)
        {
            try
            {
                var raw = Convert.FromBase64String(key);
                if (raw.Length < 32)
                {
                    throw new InvalidOperationException("JWT key must be at least 256 bits (Base64).");
                }
                return raw;
            }
            catch
            {
                var raw = Encoding.UTF8.GetBytes(key);
                if (raw.Length < 32)
                {
                    throw new InvalidOperationException("JWT key must be at least 32 bytes when using plain text.");
                }
                return raw;
            }
        }

    }

    internal sealed class DateTimeProvider : IDateTimeProvider { public DateTime UtcNow => DateTime.UtcNow; }
}