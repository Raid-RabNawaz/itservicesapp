using Hangfire;
using ITServicesApp.API.Middlewares;
using ITServicesApp.API.Observability.HealthChecks;
using ITServicesApp.Application.Interfaces.Security;
using ITServicesApp.Infrastructure.Services.Notifications;
using ITServicesApp.Persistence;
using ITServicesApp.Persistence.Seeds;
using Microsoft.AspNetCore.Localization;
using Serilog;
using System.Globalization;

namespace ITServicesApp.API.Configuration.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static WebApplication UseApiPipeline(this WebApplication app)
        {
            app.UseSerilogRequestLogging(opts =>
            {
                // (Optional) customize the message template:
                // opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

                opts.EnrichDiagnosticContext = (diag, http) =>
                {
                    diag.Set("RequestHost", http.Request.Host.Value);
                    diag.Set("RequestScheme", http.Request.Scheme);
                    diag.Set("RemoteIp", http.Connection.RemoteIpAddress?.ToString());
                    diag.Set("UserAgent", http.Request.Headers.UserAgent.ToString());
                    diag.Set("CorrelationId", http.TraceIdentifier);
                    if (http.User?.Identity?.IsAuthenticated == true)
                        diag.Set("UserName", http.User.Identity!.Name);
                };
            });


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Localization
            var supported = new[] { "en", "de" };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en"),
                SupportedCultures = supported.Select(c => new CultureInfo(c)).ToList(),
                SupportedUICultures = supported.Select(c => new CultureInfo(c)).ToList()
            });

            // CORS (if you registered "frontend" policy in DI)
            app.UseCors("frontend");

            // Rate limiting
            app.UseRateLimiter();

            // SignalR hub
            app.MapHub<NotificationHub>("/hubs/notifications");

            // Middlewares
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            // Endpoints
            app.MapControllers();

            // Health checks
            app.MapHealthChecks("/health", HealthCheckResponseWriter.Options);

            // Hangfire dashboard
            app.UseHangfireDashboard("/jobs");

            // --- Seed on startup ---
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
                var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("ApplicationSeeder");
                ApplicationSeeder.SeedAsync(db, hasher, logger).GetAwaiter().GetResult();
            }

            return app;
        }
    }
}
