using Microsoft.OpenApi.Models;

namespace SimitConsulta.API.Extensions;

/// <summary>
/// Extension methods para configurar Swagger/OpenAPI.
/// Solo activo en Development — no se expone en producción.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Registra Swagger con la información del proyecto.
    /// </summary>
    public static IServiceCollection AddSwaggerConfig(
        this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SimitConsulta API",
                Version = "v1",
                Description =
                    "REST API for querying vehicle fines and summons " +
                    "from the SIMIT (Colombian traffic violations system). " +
                    "Technical test — Automation Analyst N2."
            });
        });

        return services;
    }

    /// <summary>
    /// Habilita la UI de Swagger en /swagger.
    /// </summary>
    public static WebApplication UseSwaggerConfig(
        this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "SimitConsulta API v1");
            c.RoutePrefix = "swagger";
        });

        return app;
    }
}