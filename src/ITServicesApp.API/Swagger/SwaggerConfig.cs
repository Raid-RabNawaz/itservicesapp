using Microsoft.OpenApi.Models;

namespace ITServicesApp.API.Swagger
{
    public static class SwaggerConfig
    {
        public static void Configure(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions c)
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ITServicesApp API", Version = "v1" });
            var scheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Input your JWT token"
            };
            c.AddSecurityDefinition("Bearer", scheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { scheme, new string[] { } }
            });
        }
    }
}
