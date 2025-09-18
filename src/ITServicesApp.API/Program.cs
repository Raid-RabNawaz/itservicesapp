using ITServicesApp.API.Configuration.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
           .ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext()
           .Enrich.WithEnvironmentName()
           .Enrich.WithMachineName()
           .Enrich.WithProperty("Application", "ITServicesApp.API")
           .WriteTo.Console() // fallback if config section missing
       );

    builder.Services.AddApiServices(builder.Configuration);
    builder.Services.AddJwtAndSignalRAuth(builder.Configuration);

    var app = builder.Build();
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();  // keep HTTPS redirect only in prod
    }
    app.UseApiPipeline();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
